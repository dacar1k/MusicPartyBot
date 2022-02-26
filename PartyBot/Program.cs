using System.Threading.Tasks;
using MusicStreaming.Services;

namespace MusicStreaming
{
    class Program
    {  
        private static Task Main() => new DiscordService().InitializeAsync();
    }
}
