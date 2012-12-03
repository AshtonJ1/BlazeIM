using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.IO;
using System.Media;
using System.Threading;
using BlazeGames.Networking;
using BlazeGames.IM.Client.Networking;
using NSpeex;

namespace BlazeGames.IM.Client
{
    internal class VoiceCallCore
    {
        public int UDPPort;

        public bool SoundInEnabled = true;

        private Socket UdpListener;
        private EndPoint Any;
        private byte[] TmpBuffer = new byte[20480];

        public Dictionary<IPEndPoint, Call> CurrentCalls = new Dictionary<IPEndPoint, Call>();

        private WaveInEvent SoundIn;
        private WaveOutEvent SoundOut;
        private JitterBufferWaveProvider SoundOutProvider;
        SpeexEncoder encoder = new SpeexEncoder(BandMode.Wide);

        public VoiceCallCore()
        {
            UDPPort = new Random().Next(25050, 26050);

            Any = new IPEndPoint(IPAddress.Any, UDPPort);

            SoundOutProvider = new JitterBufferWaveProvider();

            SoundOut = new WaveOutEvent();
            SoundOut.Init(SoundOutProvider);
            SoundOut.Play();

            try
            {
                SoundIn = new WaveInEvent();
                SoundIn.WaveFormat = new WaveFormat(encoder.FrameSize * 50, 16, 1);
                SoundIn.DataAvailable += SoundIn_DataAvailable;
                SoundIn.BufferMilliseconds = 40;
                //SoundIn.StartRecording();
            }
            catch { SoundInEnabled = false; }

            UdpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //UdpListener.SendTo(new byte[] { 0x0 }, new IPEndPoint(IPAddress.Broadcast, UDPPort));
            UdpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpListener.Bind(new IPEndPoint(IPAddress.Any, UDPPort));

            UdpListener.BeginReceiveFrom(TmpBuffer, 0, 20480, SocketFlags.None, ref Any, DoReceiveFrom, null);
            
        }

        public void EndCall(int MemberID)
        {
            IPEndPoint[] tmpcalls = CurrentCalls.Keys.ToArray();

            foreach (IPEndPoint call_ep in tmpcalls)
            {
                if (MemberID == CurrentCalls[call_ep].MemberID)
                {
                    Console.WriteLine("Ending Call With " + call_ep.ToString());
                    CurrentCalls[call_ep].Dispose();
                    CurrentCalls.Remove(call_ep);
                }
            }

            if (CurrentCalls.Count <= 0)
                try
                {
                    SoundIn.StopRecording();
                }
                catch { }
        }

        public void StartCall(int MemberID, IPEndPoint ep)
        {
            if (CurrentCalls.ContainsKey(ep))
            {
                Console.WriteLine("Ending Call With " + ep.ToString());
                CurrentCalls[ep].Dispose();
                CurrentCalls.Remove(ep);
            }

            Console.WriteLine("Starting Call With " + ep.ToString());
            CurrentCalls.Add(ep, new Call(ep) { MemberID = MemberID, UdpListener = UdpListener });

            if (CurrentCalls.Count > 0)
                try
                {
                    SoundIn.StartRecording();
                }
                catch { }
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

                arrfile[j] = newval[0];
                arrfile[j + 1] = newval[1];
            }

            return arrfile;
        }

        private void SoundIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (CurrentCalls.Count <= 0)
                return;

            short[] data = new short[e.BytesRecorded / 2];
            Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesRecorded);
            var encodedData = new byte[e.BytesRecorded];
            var encodedBytes = encoder.Encode(data, 0, data.Length, encodedData, 0, encodedData.Length);
            if (encodedBytes != 0)
            {
                var upstreamFrame = new byte[encodedBytes];
                Array.Copy(encodedData, upstreamFrame, encodedBytes);

                //SoundOutProvider.Write(upstreamFrame, 0, upstreamFrame.Length);

                Call[] tmpcalls = CurrentCalls.Values.ToArray();

                foreach (Call call in tmpcalls)
                    call.SendData(upstreamFrame);
            }

            //byte[] Loud = ChangeVolume(e.Buffer, true, 50);    // 50% Increase
        }

        private void DoReceiveFrom(IAsyncResult iar)
        {
            try
            {
                EndPoint RemoteEP = new IPEndPoint(IPAddress.Any, UDPPort);

                int ReceivedLen = UdpListener.EndReceiveFrom(iar, ref RemoteEP);
                byte[] Data = new Byte[ReceivedLen];
                Array.Copy(TmpBuffer, Data, ReceivedLen);

                UdpListener.BeginReceiveFrom(TmpBuffer, 0, 20480, SocketFlags.None, ref Any, DoReceiveFrom, null);

                if (CurrentCalls.ContainsKey((IPEndPoint)RemoteEP))
                {
                    CurrentCalls[(IPEndPoint)RemoteEP].PlayData(Data);
                }
            }
            catch
            {
                CurrentCalls.Clear();
                UdpListener.Dispose();

                UdpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UdpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                UdpListener.Bind(new IPEndPoint(IPAddress.Any, UDPPort));

                UdpListener.BeginReceiveFrom(TmpBuffer, 0, 20480, SocketFlags.None, ref Any, DoReceiveFrom, null); 
            }
        }
    }

    internal class Call : IDisposable
    {
        public UdpClient UdpSender;
        public Socket UdpListener;

        public bool Calling = true;

        public int MemberID;

        public IPEndPoint Address;
        public bool SoundInMuted = false;
        public bool SoundOutMuted = false;

        private WaveOutEvent SoundOut;

        private SpeexEncoder _encoder;
        private JitterBufferWaveProvider SpeexProvider;

        public Call(IPEndPoint Address)
        {
            UdpSender = new UdpClient();
            UdpSender.Connect(Address);
            this.Address = Address;

            _encoder = new SpeexEncoder(BandMode.Wide);
            SpeexProvider = new JitterBufferWaveProvider();

            

            SoundOut = new WaveOutEvent();
            SoundOut.Init(SpeexProvider);
            SoundOut.Play();
        }

        private byte[] _notEncodedBuffer = new byte[0];
        public void SendData(byte[] data)
        {
                if (SoundInMuted)
                    return;
                else
                {
	                UdpListener.SendTo(data, data.Length, SocketFlags.None, Address);
                }
        }

        public void PlayData(byte[] data)
        {
            if (SoundOutMuted)
                return;
            else
            {
                SpeexProvider.Write(data, 0, data.Length);
                Console.Write("*");
            }
        }

        /* GC */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SoundOut.Dispose();
                SpeexProvider.Dispose();
            }
        }
    }

    public class VolumeUpdatedEventArgs : EventArgs
    {
        public int Volume { get; set; }
    }

    public class JitterBufferWaveProvider : WaveStream
    {
        private readonly SpeexDecoder decoder = new SpeexDecoder(BandMode.Narrow);
        private readonly SpeexJitterBuffer jitterBuffer;

        //private readonly NativeDecoder decoder = new NativeDecoder((EncodingMode)1);
        //private readonly NativeJitterBuffer jitterBuffer;

        private readonly WaveFormat waveFormat;
        private readonly object readWriteLock = new object();

        public JitterBufferWaveProvider()
        {
            waveFormat = new WaveFormat(decoder.FrameSize * 50, 16, 1);
            jitterBuffer = new SpeexJitterBuffer(decoder);
            //jitterBuffer = new NativeJitterBuffer(decoder);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int peakVolume = 0;
            int bytesRead = 0;
            lock (readWriteLock)
            {
                while (bytesRead < count)
                {
                    if (exceedingBytes.Count != 0)
                    {
                        buffer[bytesRead++] = exceedingBytes.Dequeue();
                    }
                    else
                    {
                        short[] decodedBuffer = new short[decoder.FrameSize * 2];
                        jitterBuffer.Get(decodedBuffer);
                        for (int i = 0; i < decodedBuffer.Length; ++i)
                        {
                            if (bytesRead < count)
                            {
                                short currentSample = decodedBuffer[i];
                                peakVolume = currentSample > peakVolume ? currentSample : peakVolume;
                                BitConverter.GetBytes(currentSample).CopyTo(buffer, offset + bytesRead);
                                bytesRead += 2;
                            }
                            else
                            {
                                var bytes = BitConverter.GetBytes(decodedBuffer[i]);
                                exceedingBytes.Enqueue(bytes[0]);
                                exceedingBytes.Enqueue(bytes[1]);
                            }
                        }
                    }
                }
            }

            OnVolumeUpdated(peakVolume);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (readWriteLock)
            {
                jitterBuffer.Put(buffer);
            }
        }

        public override long Length
        {
            get { return 1; }
        }

        public override long Position
        {
            get { return 0; }
            set { throw new NotImplementedException(); }
        }

        public override WaveFormat WaveFormat
        {
            get
            {
                return waveFormat;
            }
        }

        public EventHandler<VolumeUpdatedEventArgs> VolumeUpdated;

        private void OnVolumeUpdated(int volume)
        {
            var eventHandler = VolumeUpdated;
            if (eventHandler != null)
            {
                eventHandler.BeginInvoke(this, new VolumeUpdatedEventArgs { Volume = volume }, null, null);
            }
        }

        private readonly Queue<byte> exceedingBytes = new Queue<byte>();
    }
}
