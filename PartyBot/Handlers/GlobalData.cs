using Discord;
using Newtonsoft.Json;
using MusicStreaming.DataStructs;
using MusicStreaming.Services;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MusicStreaming.Handlers
{
    public class GlobalData
    {
        public static string ConfigPath { get; set; } = "config.json";
        public static BotConfig Config { get; set; }

        public async Task InitializeAsync()
        {
            var json = string.Empty;
                        
            if (!File.Exists(ConfigPath))
            {
                json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
                File.WriteAllText("config.json", json, new UTF8Encoding(false));
                await LoggingService.LogAsync("Bot", LogSeverity.Error, "No Config file found. A new one has been generated. Please close the & fill in the required section.");
                await Task.Delay(-1);
            }

            json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<BotConfig>(json);
        }

        private static BotConfig GenerateNewConfig() => new BotConfig
        {
            DiscordToken = "put token here",
            DefaultPrefix = "-"
        };
    }
}
