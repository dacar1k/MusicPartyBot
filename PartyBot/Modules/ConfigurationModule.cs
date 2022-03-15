using Discord;
using Discord.Commands;
using Infrastructure;
using MusicStreaming.Services;
using MusicStreaming.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;

namespace MusicStreaming.Modules
{
    public class ConfigurationModule : ModuleBase<SocketCommandContext>
    {

        public ConfigurationService ConfigService { get; set; }
        public LavaLinkAudio AudioService { get; set; }
        private readonly Servers _servers;
        private readonly Tracks _tracks;   
        

        public ConfigurationModule(Servers servers, Tracks tracks)
        {
            _servers = servers;
            _tracks = tracks;
        }

        [Command("prefix", RunMode = RunMode.Async)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "-";
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}`.");
                return;
            }
            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been adjusted to `{prefix}`.");
        }

        [Command("createpl"),Alias("crpl")]
        public async Task CreatePlaylist(string name,[Remainder]string desc)
        {
            await ReplyAsync(await ConfigService.CreatePlaylist(Context.Guild, name, desc));
            await ReplyAsync(embed: await ConfigService.ShowPlayList(Context.Guild));
        }             

        [Command("showplaylist"), Alias("shpl")]
        public async Task ShowPlaylist(string Name)
            => await ReplyAsync(embed: await ConfigService.ShowPlaylist(Context.Guild, Name));

        [Command("allplaylist"), Alias("allpl")]
        public async Task ShowAllPlaylists()
            => await ReplyAsync(embed: await ConfigService.ShowPlayList(Context.Guild));

        [Command("rmplaylist"), Alias("rmpl")]
        public async Task DeletePlayList(string name)
        {
            await (ReplyAsync(await ConfigService.DeletePlayList(Context.Guild, name)));
        }

        [Command("addtrack"), Alias("addtr")]
        public async Task AddTrack(string pltitle, [Remainder]string track)
        {
            var _track = await AudioService.Search(track);
            await _tracks.AddTrack(Context.Guild.Id, pltitle, _track.Tracks.FirstOrDefault().Title, _track.Tracks.FirstOrDefault().Url);
        }
         
        [Command("rmtrack"), Alias("rmtr")]
        public async Task RemoveTrack(string name, [Remainder] string title)
            => await _tracks.RemoveTrack(Context.Guild.Id, name, title);

        [Command("help"), Alias("h")]
        public async Task ShowHelp()
            => await ConfigService.HelpAsync(Context.User as SocketGuildUser);
    }
}
