using Discord;
using Discord.Commands;
using Infrastructure;
using MusicStreaming.Services;
using MusicStreaming.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MusicStreaming.Modules
{
    public class ConfigurationModule : ModuleBase<SocketCommandContext>
    {

        public ConfigurationService ConfigService { get; set; }
        private readonly Servers _servers;
        private readonly PlayLists _playlists;
        private readonly Tracks _tracks;

        public ConfigurationModule(Servers servers, PlayLists playlists, Tracks tracks)
        {
            _servers = servers;
            _playlists = playlists;
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

        [Command("setstatus")]
        [Alias("status")]
        public async Task Status(string status = null)
        {
            await _servers.SetStatus(Context.Guild.Id, status);
            await ReplyAsync($"The Status has been adjusted to `{status}`.");
        }

        [Command("crp")]
        public async Task CreatePlaylist(string name, string desc)
            => await ConfigService.CreatePlaylist(Context.Guild, name, desc);

        [Command("showplaylist"), Alias("spl")]
        public async Task ShowPlaylist(string Name)
            => await ReplyAsync(embed: await ConfigService.ShowPlaylist(Context.Guild, Name));

        [Command("playlists"), Alias("allpl")]
        public async Task ShowAllPlaylists()
            => await ReplyAsync(embed: await ConfigService.ShowPlayList(Context.Guild));

        [Command("removepl"), Alias("rmpl")]
        public async Task DeletePlayList(string name)
            => await (ReplyAsync(await ConfigService.DeletePlayList(Context.Guild, name)));

        [Command("addtrack"), Alias("addtr")]
        public async Task AddTrack(string plname, string title, string link)
        {
            await _tracks.AddTrack(plname, title, link);
        }

        //[Command("removetr"),Alias("rmtr")]
        //public async
    }
}
