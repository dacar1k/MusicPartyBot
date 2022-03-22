using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MusicStreaming.Services;
using System.Threading.Tasks;

namespace MusicStreaming.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {     
        public LavaLinkAudio AudioService { get; set; }

        [Command("remove"), Alias("rm")]
        public async Task RemoveTrack(int id)
            => await ReplyAsync(embed: await AudioService.RemoveAsync( Context.Guild, id));

        [Command("join"),Alias("j")]
        public async Task JoinAndPlay()
            => await ReplyAsync(embed: await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("leave"),Alias("lv")]
        public async Task Leave()
            => await ReplyAsync(embed: await AudioService.LeaveAsync(Context.Guild));

        [Command("play"),Alias("p")]
        public async Task Play([Remainder]string search)
            => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("stop")]
        public async Task Stop()
            => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild));

        [Command("queue"),Alias("q")]
        public async Task List()
            => await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild));

        [Command("skip"), Alias("n")]
        public async Task Skip()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild, true));

        [Command("back"), Alias("b")]
        public async Task Back()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild, false));

        [Command("volume")]
        public async Task Volume(int volume)
            => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume));

        [Command("pause")]
        public async Task Pause()
            => await ReplyAsync(await AudioService.PauseAsync(Context.Guild));

        [Command("resume")]
        public async Task Resume()
            => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild));

        [Command("loadpl"), Alias("ldpl")]
        public async Task LoadPlayList(ulong id)     
            => await ReplyAsync(embed: await AudioService.LoadPL(Context.User as SocketGuildUser, Context.User as IVoiceState, Context.Channel as ITextChannel, Context.Guild, id));

        [Command("shuffle", RunMode = RunMode.Async), Alias("sh")]
        public async Task Shuffle()
        {
            await AudioService.Shaffle(Context.Guild);
            await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild));
        }
        [Command("search", RunMode = RunMode.Async), Alias("sr")]
        public async Task Finde(string name)
        {
            await AudioService.FindbyType(Context.User as SocketGuildUser, Context.Guild, name, Context.User as IVoiceState, Context.Channel as ITextChannel);
            await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild));
        }
    }
}
