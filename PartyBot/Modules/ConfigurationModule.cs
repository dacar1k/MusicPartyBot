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
        private readonly Tracks _tracks;

        public ConfigurationModule(Servers servers, PlayLists playlists, Tracks playListQueue)
        {
            _servers = servers;
            _playlists = playlists;
            _tracks = playListQueue;
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
        public async Task CreatePlaylist(string name)
        {
            await _playlists.CreatePlaylist(Context.Guild.Id, name.Split()[0], name.Split()[1]);
        }

        [Command("spl")]
        public async Task ShowPlaylist()
        {
            var descriptionBuilder = new StringBuilder();
            var playlistNum = 0;
            List<PlayList> pl = await _playlists.ShowPlayLists(Context.Guild.Id);
            if (pl != null)
            {
                foreach (PlayList playlist in pl)
                {
                    descriptionBuilder.Append($"{playlistNum}: {playlist.Name}");
                    playlistNum++;
                }
                await EmbedHandler.CreateBasicEmbed("Playlists", $"{descriptionBuilder}", Color.Blue);
            }
            else
                await EmbedHandler.CreateErrorEmbed("Playlist", "playlists were not found");

        }

        [Command("showplaylist")]
        public async Task ShowPlaylist(string Name)
        {
            var descriptionBuilder = new StringBuilder();
            var trackNum = 0;
            List<Track> tracks = await _tracks.ShowTracks(Name);
            if(tracks != null)
            {
                foreach(Track track in tracks)
                {
                    descriptionBuilder.Append($"{trackNum}: {track.Title} - {track.Link}");                    
                }
                await EmbedHandler.CreateBasicEmbed($"Playlist {Name}", $"{descriptionBuilder}", Color.Blue);
            }
            else
                await EmbedHandler.CreateErrorEmbed($"Playlist {Name}", "Tracks were not found");
        }

        [Command("AddTrack"),Alias("at")]
        public async Task AddTrack(string msg)
        {
            await _tracks.AddTrack(msg.Split()[0], msg.Split()[1], msg.Split()[2]);
        }
    }
}
