using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;

namespace BlazeGames.IM.Client
{
    internal class SoundManager
    {
        public static SoundPlayer VoiceCallingSound = new SoundPlayer(BlazeGames.IM.Client.Properties.Resources.Phone_calling_tone);
        public static SoundPlayer VoiceRingingSound = new SoundPlayer(BlazeGames.IM.Client.Properties.Resources.ringtone_slow);
        public static SoundPlayer NewMessageSound = new SoundPlayer(BlazeGames.IM.Client.Properties.Resources.new_message_bells);
    }
}
