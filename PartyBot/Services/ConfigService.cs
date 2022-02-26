using System;
using System.Collections.Generic;
using System.Text;
using Victoria;

namespace MusicStreaming.Services
{
    public sealed class ConfigService
    {
        private readonly LavaNode _lavaNode;
        public ConfigService(LavaNode lavaNode) => _lavaNode = lavaNode;       
    }
}
