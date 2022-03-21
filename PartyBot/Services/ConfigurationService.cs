using Discord;
using Discord.WebSocket;
using Infrastructure;
using MusicStreaming.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace MusicStreaming.Services
{
    public sealed class ConfigurationService
    {
        private readonly PlayLists _playlists;
        private readonly Tracks _tracks; 

        public ConfigurationService(PlayLists playLists, Tracks tracks)
        {
            _playlists = playLists;
            _tracks = tracks;
        }
        public async Task<Embed> HelpAsync(SocketGuildUser user)
        {
            try
            {
                StringBuilder msgBuilder = new StringBuilder();
                var msg = GlobalData.Config.Help;
                foreach (var str in msg) { msgBuilder.Append($"{str}\n"); }
                await user.SendMessageAsync($">>> {msgBuilder}");
                return await EmbedHandler.CreateBasicEmbed("Help", "Please check your direct message", Color.Magenta);
            }
            catch { return await EmbedHandler.CreateErrorEmbed("Error", "something went wrong"); }
            
        }

        public async Task<Embed> ShowPlayList(IGuild guild)
        {
            var descriptionBuilder = new StringBuilder();
            var playlistNum = 1;
            var pl = await _playlists.ShowPlayLists(guild.Id);
            if ((pl != null) || (pl.Count != 0))
            {
                descriptionBuilder.Append("№: ID   Name  -  Description\n");
                foreach (PlayList playlist in pl)
                {
                    descriptionBuilder.Append($"{playlistNum}: {playlist.Id}  {playlist.Name} - {playlist.Description} \n");
                    playlistNum++;
                }
                return await EmbedHandler.CreateBasicEmbed("Playlists", $"{descriptionBuilder}", Color.Blue);
            }
            else
                return await EmbedHandler.CreateErrorEmbed("Playlist", "playlists were not found");            
        }

        public async Task<Embed> ShowPlaylist(IGuild guild, ulong id)
        {
            var descriptionBuilder = new StringBuilder();
            var trackNum = 1;
            string Name = await _playlists.GetName(guild.Id, id);
            List<Track> tracks = await _tracks.GetTracks(guild.Id, id);
            if (tracks != null)
            {
                foreach (Track track in tracks)
                {
                    descriptionBuilder.Append($"{trackNum}: {track.Title}\n"); 
                    trackNum++;
                }
                return await EmbedHandler.CreateBasicEmbed($"Playlist {Name}", $"{descriptionBuilder} ", Color.Blue);
            }
            else
                return await EmbedHandler.CreateErrorEmbed($"Playlist {Name}", "Tracks were not found");
        }

        public async Task<string> CreatePlaylist(IGuild guid, string name, string desc)
        {
            bool check = await _playlists.CheckIfExists(guid.Id, name);
            if (check == true) return "Playlist with the name already exists in database, try to give other name";
            else
            {
                await _playlists.CreatePlaylist(guid.Id, name, desc);
                return "Playlist was successfuly created";
            }
        }

        public async Task<string> DeletePlayList(IGuild guild, ulong ID)
        {
            return await _playlists.DeletePlaylist(guild.Id, ID);
        }
        

    }
}
