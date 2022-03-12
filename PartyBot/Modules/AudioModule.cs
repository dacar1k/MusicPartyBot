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

        [Command("Join"),Alias("j")]
        public async Task JoinAndPlay()
            => await ReplyAsync(embed: await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("Leave"),Alias("lv")]
        public async Task Leave()
            => await ReplyAsync(embed: await AudioService.LeaveAsync(Context.Guild));

        [Command("Play"),Alias("p")]
        public async Task Play([Remainder]string search)
            => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild));

        [Command("List"),Alias("q")]
        public async Task List()
            => await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild));

        [Command("Skip"), Alias("n")]
        public async Task Skip()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild, true));

        [Command("Back"), Alias("b")]
        public async Task Back()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild,false));

        [Command("Volume")]
        public async Task Volume(int volume)
            => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await AudioService.PauseAsync(Context.Guild));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild));

        [Command("loadpl"), Alias("lpl")]
        public async Task LoadPlayList(string name)     
            => await ReplyAsync(embed: await AudioService.LoadPL(Context.User as SocketGuildUser, Context.User as IVoiceState, Context.Channel as ITextChannel, Context.Guild, name));

        [Command("Loop")]
        public async Task Loop()
            => await Context.Channel.SendMessageAsync(await AudioService.LoopAsync(Context.Guild));
    }
}
