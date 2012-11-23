using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Audio.Codecs;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.IO;
using System.Media;
using System.Threading;

namespace BlazeGames.IM.Client
{
    internal class VoiceCallCore
    {
        public int UDPPort;

        public bool SoundInEnabled = true;

        private System.Net.Sockets.Socket UdpSocket;
        private EndPoint Any;
        private byte[] TmpBuffer = new byte[20480];

        public Dictionary<IPEndPoint, Call> CurrentCalls = new Dictionary<IPEndPoint, Call>();

        private WaveInEvent SoundIn;
        private WaveOutEvent SoundOut;
        private BufferedWaveProvider SoundOutProvider;

        public VoiceCallCore()
        {
            UDPPort = new Random().Next(25050, 26050);

            Any = new IPEndPoint(IPAddress.Any, UDPPort);

            SoundOutProvider = new BufferedWaveProvider(new WaveFormat(48000, 1));

            SoundOut = new WaveOutEvent();
            SoundOut.Init(SoundOutProvider);
            SoundOut.Play();

            try
            {
                SoundIn = new WaveInEvent();
                SoundIn.WaveFormat = new WaveFormat(48000, 1);
                SoundIn.DataAvailable += SoundIn_DataAvailable;
                SoundIn.BufferMilliseconds = 100;
                SoundIn.StartRecording();
            }
            catch { SoundInEnabled = false; }

            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UdpSocket.Bind(new IPEndPoint(IPAddress.Any, UDPPort));

            UdpSocket.BeginReceiveFrom(TmpBuffer, 0, 20480, SocketFlags.None, ref Any, DoReceiveFrom, null);
            
        }

        public void EndCall(int MemberID)
        {
            IPEndPoint[] tmpcalls = CurrentCalls.Keys.ToArray();

            foreach (IPEndPoint call_ep in tmpcalls)
            {
                if (MemberID == CurrentCalls[call_ep].MemberID)
                {
                    Console.WriteLine("Ending Call With " + call_ep.ToString());
                    CurrentCalls.Remove(call_ep);
                }
            }
        }

        public void StartCall(int MemberID, IPEndPoint ep)
        {
            if (CurrentCalls.ContainsKey(ep))
            {
                Console.WriteLine("Ending Call With " + ep.ToString());
                CurrentCalls.Remove(ep);
            }

            Console.WriteLine("Starting Call With " + ep.ToString());
            CurrentCalls.Add(ep, new Call { MemberID = MemberID, Address = ep, UdpSocket = UdpSocket });
        }

        private short ComplementToSigned(ref byte[] bytArr, int intPos)
        {
            short snd = BitConverter.ToInt16(bytArr, intPos);
            if (snd != 0)
                snd = Convert.ToInt16((~snd | 1));
            return snd;
        }

        /// <summary>
        /// Convert signed sample value back to 2's complement value equivalent to Stereo. This method is used 
        /// by other public methods to equilibrate wave formats of different files.
        /// </summary>
        /// <param name="shtVal">The mono signed value as short</param>
        /// <returns>Stereo 2's complement value as byte array</returns>
        private byte[] SignedToComplement(short shtVal) //Convert to 2's complement and return as byte array of 2 bytes
        {
            byte[] bt = new byte[2];
            shtVal = Convert.ToInt16((~shtVal | 1));
            bt = BitConverter.GetBytes(shtVal);
            return bt;
        }

        /// <summary>
        /// Increase or decrease volume of a wave file by percentage
        /// </summary>
        /// <param name="strPath">Source wave</param>
        /// <param name="booIncrease">True - Increase, False - Decrease</param>
        /// <param name="shtPcnt">1-100 in %-age</param>
        /// <returns>True/False</returns>
        public byte[] ChangeVolume(byte[] data, bool booIncrease, short shtPcnt)
        {
            if (shtPcnt > 100)
                throw new ArgumentOutOfRangeException("shtPcnt", shtPcnt, "Value was greater than 100%");

            byte[] arrfile = data;


            //change volume
            for (int j = 0; j < arrfile.Length; j += 2)
            {
                short snd = ComplementToSigned(ref arrfile, j);
                try
                {
                    short p = Convert.ToInt16((snd * shtPcnt) / 100);
                    if (booIncrease)
                        snd += p;
                    else
                        snd -= p;
                }
                catch
                {
                    snd = ComplementToSigned(ref arrfile, j);
                }
                byte[] newval = SignedToComplement(snd);
                if ((newval[0] != null) && (newval[1] != null))
                {
                    arrfile[j] = newval[0];
                    arrfile[j + 1] = newval[1];
                }
            }

            return arrfile;
        }

        private void SoundIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] Loud = ChangeVolume(e.Buffer, true, 50);    // 50% Increase
            Call[] tmpcalls = CurrentCalls.Values.ToArray();

            foreach (Call call in tmpcalls)
            {
                call.SendData(Loud);
            }
        }

        private void DoReceiveFrom(IAsyncResult iar)
        {
            try
            {
                EndPoint RemoteEP = new IPEndPoint(IPAddress.Any, UDPPort);

                int ReceivedLen = UdpSocket.EndReceiveFrom(iar, ref RemoteEP);
                byte[] Data = new Byte[ReceivedLen];
                Array.Copy(TmpBuffer, Data, ReceivedLen);

                UdpSocket.BeginReceiveFrom(TmpBuffer, 0, 20480, SocketFlags.None, ref Any, DoReceiveFrom, null);

                if (CurrentCalls.ContainsKey((IPEndPoint)RemoteEP))
                {
                    CurrentCalls[(IPEndPoint)RemoteEP].PlayData(Data);
                }
            }
            catch
            {
                CurrentCalls.Clear();
                UdpSocket.Dispose();

                UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UdpSocket.Bind(new IPEndPoint(IPAddress.Any, UDPPort));

                UdpSocket.BeginReceiveFrom(TmpBuffer, 0, 20480, SocketFlags.None, ref Any, DoReceiveFrom, null); 
            }
        }
    }

    internal class Call
    {
        public bool Calling = true;

        public int MemberID;

        public IPEndPoint Address;
        public Socket UdpSocket;
        public bool SoundInMuted = false;
        public bool SoundOutMuted = false;

        private WaveOutEvent SoundOut;
        private BufferedWaveProvider SoundOutProvider;

        private OpusEncoder _encoder;
        private OpusDecoder _decoder;
        private int _segmentFrames;
        private int _bytesPerSegment;

        public Call()
        {
            _segmentFrames = 960;
            _encoder = OpusEncoder.Create(48000, 1, Audio.Codecs.Opus.Application.Voip, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "Codecs"));
            _encoder.Bitrate = 8192;
            _decoder = OpusDecoder.Create(48000, 1, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlazeGamesIM", "Codecs"));
            _bytesPerSegment = _encoder.FrameByteCount(_segmentFrames);

            SoundOutProvider = new BufferedWaveProvider(new WaveFormat(48000, 1));

            SoundOut = new WaveOutEvent();
            SoundOut.Init(SoundOutProvider);
            SoundOut.Play();
        }

        private byte[] _notEncodedBuffer = new byte[0];
        public void SendData(byte[] data)
        {
                if (SoundInMuted)
                    return;
                else
                {
                    byte[] soundBuffer = new byte[data.Length + _notEncodedBuffer.Length];
                    for (int i = 0; i < _notEncodedBuffer.Length; i++)
                        soundBuffer[i] = _notEncodedBuffer[i];
                    for (int i = 0; i < data.Length; i++)
                        soundBuffer[i + _notEncodedBuffer.Length] = data[i];

                    int byteCap = _bytesPerSegment;
                    int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
                    int segmentsEnd = segmentCount * byteCap;
                    int notEncodedCount = soundBuffer.Length - segmentsEnd;
                    _notEncodedBuffer = new byte[notEncodedCount];
                    for (int i = 0; i < notEncodedCount; i++)
                    {
                        _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
                    }

                    for (int i = 0; i < segmentCount; i++)
                    {
                        byte[] segment = new byte[byteCap];
                        for (int j = 0; j < segment.Length; j++)
                            segment[j] = soundBuffer[(i * byteCap) + j];

                        int len;
                        byte[] buff = _encoder.Encode(segment, segment.Length, out len);

                        try
                        {
                            UdpSocket.SendTo(buff, len, SocketFlags.None, Address);
                        }
                        catch { }
                    }
                }
        }

        public void PlayData(byte[] data)
        {
            if (SoundOutMuted)
                return;
            else
            {
                int length;
                byte[] buff = _decoder.Decode(data, data.Length, out length);

                SoundOutProvider.AddSamples(buff, 0, length);
            }
        }
    }
}
