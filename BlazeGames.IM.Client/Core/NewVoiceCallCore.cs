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
    internal class NewVoiceCallCore
    {
        public bool SoundInEnabled = true;
        public Dictionary<string, NewCall> CurrentCalls = new Dictionary<string, NewCall>();
        private WaveInEvent SoundIn;
        SpeexEncoder encoder = new SpeexEncoder(BandMode.Wide);

        public NewVoiceCallCore()
        {
            try
            {
                SoundIn = new WaveInEvent();
                SoundIn.WaveFormat = new WaveFormat(encoder.FrameSize * 50, 16, 1);
                SoundIn.DataAvailable += SoundIn_DataAvailable;
                SoundIn.BufferMilliseconds = 40;
            }
            catch { SoundInEnabled = false; }
            
        }

        public void EndCall(int MemberID)
        {
            string[] tmpcalls = CurrentCalls.Keys.ToArray();

            foreach (string call_ep in tmpcalls)
            {
                if (MemberID == CurrentCalls[call_ep].MemberID)
                {
                    Console.WriteLine("Ending Call With " + call_ep.ToString());
                    CurrentCalls[call_ep].End();
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

        public void StartCall(int MemberID, string CallID)
        {
            EndCall(MemberID);

            Console.WriteLine("Starting Call With " + CallID);
            CurrentCalls.Add(CallID, new NewCall(null) { MemberID = MemberID });

            if (CurrentCalls.Count > 0)
                try
                {
                    SoundIn.StartRecording();
                }
                catch { }
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

                NewCall[] tmpcalls = CurrentCalls.Values.ToArray();

                foreach (NewCall call in tmpcalls)
                    call.SendData(upstreamFrame);
            }

            //byte[] Loud = ChangeVolume(e.Buffer, true, 50);    // 50% Increase
        }
    }

    internal class NewCall : IDisposable
    {
        public string CallID = "";
        public UdpClient UdpConnection;

        public bool Calling = true;

        public int MemberID;

        public bool SoundInMuted = false;
        public bool SoundOutMuted = false;

        private WaveOutEvent SoundOut;

        private SpeexEncoder _encoder;
        private JitterBufferWaveProvider SpeexProvider;

        public NewCall(string CallID)
        {
            this.CallID = CallID;
            UdpConnection = new UdpClient();
            UdpConnection.Connect("209.141.53.112", 25000);

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
                    Packet pak = new Packet(null);
                    pak.Write(0x03);
                    pak.Write(CallID);
                    pak.Write(data.Length);
                    pak.Write(data);

                    UdpConnection.Send(pak.ToArray(), pak.Length);
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

        public void End()
        {

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
}
