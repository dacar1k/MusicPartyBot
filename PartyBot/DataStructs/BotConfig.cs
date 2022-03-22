using System.Collections.Generic;

namespace MusicStreaming.DataStructs
{
    public class BotConfig
    {
        public string DiscordToken { get; set; }
        public string DefaultPrefix { get; set; }
        public string GameStatus { get; set; }
        public List<string> Help { get; set; }
        public string API { get; set; }
    }
}
