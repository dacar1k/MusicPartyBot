using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria;


namespace MusicStreaming.CustomVi
{
    public class MusicPlayer : LavaPlayer
    {
        private readonly LavaSocket _lavaSocket;
        public IVoiceChannel _VoiceChannel { get; internal set; }
        public ITextChannel _TextChannel { get; internal set; }
        public bool LoopEnabled { get; set; }
        public string FilterEnabled { get; set; }
        public IUserMessage NowPlayingMessage { get; set; }
        public DefaultQueue<LavaTrack> QueueHistory { get; set; }
        public int currentplay { get; set; }
        public bool forvard { get; set; }

        public MusicPlayer(LavaSocket lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(lavaSocket,voiceChannel,textChannel)
        {
            _VoiceChannel = voiceChannel;
            _lavaSocket = lavaSocket;
            _TextChannel = textChannel;
            LoopEnabled = false;
            FilterEnabled = null;
            NowPlayingMessage = null;
            QueueHistory = new DefaultQueue<LavaTrack>();
            currentplay = 0;
            forvard = true;
        }

    }
}
