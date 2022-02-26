using Discord;
using Discord.Commands;
using Infrastructure;
using MusicStreaming.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MusicStreaming.Modules
{
    public class ConfigurationModule : ModuleBase<SocketCommandContext>
    {
        private readonly Servers _servers;
        //private readonly Discord _discord;
        private readonly PlayLists _playlists;
        private readonly Track _tracks;

        public ConfigurationModule(Servers servers, PlayLists playlists, Track playListQueue)
        {
            _servers = servers;
            _playlists = playlists;
            _tracks = playListQueue;
        }


        [Command("prefix", RunMode = RunMode.Async)]
        //[RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "-";
                //return await EmbedHandler.CreateBasicEmbed("User command", $"Current refix is {guildPrefix}", Color.Blue);
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}`.");
                return;
            }
            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been adjusted to `{prefix}`.");
            //return await EmbedHandler.CreateBasicEmbed("User command", $"The prefix has been adjusted to {prefix}", Color.Blue);
        }

        [Command("setstatus")]
        [Alias("status")]
        public async Task Status(string status = null)
        {
            await _servers.SetStatus(Context.Guild.Id, status);
            await ReplyAsync($"The Status has been adjusted to `{status}`.");
        }
        [Command("crp")]
        public async Task CreatePlaylist(string name)
        {
            await _playlists.CreatePlaylist(Context.Guild.Id, name.Split()[0], "");
        }
        [Command("spl")]
        public async Task ShowPlaylist()
        {
            List<PlayList> pl = await _playlists.ShowPlayLists(Context.Guild.Id);
            ReplyAsync($"{pl[0].Name} {pl[0].Id} {pl[0].Description}");
        }
    }
}
