using Discord;
using Discord.WebSocket;
using MusicStreaming.Handlers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Infrastructure;
using System.Collections.Generic;
using Victoria.Responses.Search;
using MusicStreaming.CustomVi;

namespace MusicStreaming.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly Tracks _tracks;
        private readonly LavaNode<MusicPlayer> _lavaNode;
        private readonly Youtube _youtube;

        public LavaLinkAudio( LavaNode<MusicPlayer> lavaNode, Tracks tracks, Youtube youtube)
        {
            _lavaNode = lavaNode;
            _tracks = tracks;    
            _youtube = youtube;
        } 

        public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
            }

            if (voiceState.VoiceChannel is null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        public async Task<SearchResponse> Search(string query)
        {
            var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                  await _lavaNode.SearchAsync(SearchType.YouTube, query)
                    : await _lavaNode.SearchYouTubeAsync(query);
            if (!search.Tracks.Any())
            {
                search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                 await _lavaNode.SearchAsync(SearchType.YouTubeMusic, query)
                    : await _lavaNode.SearchYouTubeAsync(query);
            }
            return search;
        }

        public async Task<Embed> LoadPL(SocketGuildUser user, IVoiceState voiceState, ITextChannel textChannel, IGuild guild, ulong id)
        {
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }
            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
            }
            catch { }

            var player = _lavaNode.GetPlayer(guild);

            player.Queue.Clear();
            player.QueueHistory.Clear();
            try
            {
                var tracks = await _tracks.GetTracks(guild.Id, id);
                var fr = tracks.ElementAt(0);
                tracks.RemoveAt(0);
                
                foreach (var query in tracks)
                {
                    var search = Uri.IsWellFormedUriString(query.Link, UriKind.Absolute) ?
                         await _lavaNode.SearchAsync(SearchType.YouTube, query.Link)
                         : await _lavaNode.SearchYouTubeAsync(query.Link);
                    player.Queue.Enqueue(search.Tracks.FirstOrDefault());
                }

                var first = Search(fr.Link).Result.Tracks.First();
                player.QueueHistory.Enqueue(first);
                await player.PlayAsync(first);
                return await ListAsync(guild);
            }
            catch
            {
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Plailist with id - {id} doesn't exists"); 
            }


        }
        
        public async Task FindbyType(SocketGuildUser user, IGuild guild, string x, IVoiceState voiceState, ITextChannel textChannel)
        {
            List<string> list = await _youtube.FindByType(x);


            var player = _lavaNode.GetPlayer(guild);
            if(list == null) { return; }
            player.Queue.Clear();
            player.QueueHistory.Clear();
            
            LavaTrack search;
            foreach (string item in list)
            {
                search = Search(item).Result.Tracks.FirstOrDefault();
                player.Queue.Enqueue(search);
            }
            
            var first = player.Queue.ElementAt(0);
            player.Queue.RemoveAt(0);
            player.QueueHistory.Enqueue(first);
            await player.PlayAsync(first);           
        }

        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }
            if (!_lavaNode.HasPlayer(guild))
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }

            try
            {
                var player = _lavaNode.GetPlayer(guild);

                SearchResponse search = await Search(query);

                if (!search.Tracks.Any())
                {
                    return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query} Try to use short url.");
                }

                var isPlaylist = search.Playlist.Name != null;
                var firstTrack = search.Tracks.FirstOrDefault();

                if (isPlaylist)
                {
                    var descriptionBuilder = new StringBuilder();
                    var trackNum = player.Queue.Count + player.QueueHistory.Count;
                    if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                    {
                        foreach (var track in search.Tracks)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}] - {track.Duration}\n");
                            trackNum++;
                            player.Queue.Enqueue(track);
                        }
                        return await EmbedHandler.CreateBasicEmbed("Music", $"{descriptionBuilder} have been added to the queue.", Color.Blue);
                    }
                }else
                if (!isPlaylist)
                {
                    if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                    {
                        player.Queue.Enqueue(firstTrack);
                        return await EmbedHandler.CreateBasicEmbed("Music", $"{firstTrack.Title} have been added to the queue.", Color.Blue);
                    }
                    player.QueueHistory.Enqueue(firstTrack);
                    await player.PlayAsync(firstTrack);
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{firstTrack.Title} have been added to the queue.", Color.Blue);
                }
                
                return await EmbedHandler.CreateErrorEmbed("Music, Play", "No Track found"); //пересмотреть 

            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
            }
        }

        public async Task<Embed> LeaveAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();               
                }
                player.Queue.Clear();
                player.QueueHistory.Clear();
                await _lavaNode.LeaveAsync(player.VoiceChannel);                
                await LoggingService.LogInformationAsync("Music", $"Bot has left.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing music.", Color.Blue);
            }
            catch (InvalidOperationException ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
            }
           
        }

        public async Task<Embed> ListAsync(IGuild guild)
        {
            try
            {
                var descriptionBuilder = new StringBuilder();

                var player = _lavaNode.GetPlayer(guild);

                if (player.Queue == null && player.QueueHistory == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
                var trackNum = 1;

                if (player.QueueHistory != null || player.QueueHistory.Count != 0)
                    foreach (LavaTrack track in player.QueueHistory)
                    {
                        descriptionBuilder.Append($"{trackNum}: {track.Title} - {track.Duration}\n");
                        trackNum++;
                    }
                if (player.Queue != null || player.Queue.Count != 0)
                    foreach (LavaTrack track in player.Queue)
                    {
                        descriptionBuilder.Append($"{trackNum}: {track.Title} - {track.Duration}\n");
                        trackNum++;
                    }
                if (player.PlayerState is PlayerState.Playing)
                {
                    return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: {player.Track.Title} ({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
                }
                else if (player.IsConnected)               
                {
                    return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing:  \n{descriptionBuilder}", Color.Blue);
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "something wrong");
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }

        }

        public async Task<Embed> RemoveAsync(IGuild guild, int id)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                {
                    return await EmbedHandler.CreateErrorEmbed(null, $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
                }
             

                if (id < 0 || player.Queue.Count + player.QueueHistory.Count + 2 < id)
                {
                    return await EmbedHandler.CreateErrorEmbed("List", $"Can't remove track with {id}");
                }else
                if (player.QueueHistory.Count >= id - 1)
                {
                    var buf = player.QueueHistory.ElementAt(id - 1);
                    player.QueueHistory.RemoveAt(id - 1);
                    await player.SkipAsync();
                    return await EmbedHandler.CreateBasicEmbed(null, $"Track with id {id} {buf.Title} was removed from queue", Color.Blue);
                }
                if (player.QueueHistory.Count + player.Queue.Count >= id - 1)
                {
                    var buf = player.Queue.ElementAt(id - 2);
                    player.Queue.RemoveAt(id-2);
                    return await EmbedHandler.CreateBasicEmbed(null, $"Track with id {id} {buf.Title} was removed from queue", Color.Blue);
                }
                return await EmbedHandler.CreateErrorEmbed("List", $"Can't remove track with {id}");
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }
        }   

        public async Task<Embed> SkipTrackAsync(IGuild guild, bool forvard) 
        {
            try
            {

                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                if (forvard)
                {
                    if (player.Queue.Count < 1)
                    {
                        foreach (var tr in player.QueueHistory)
                        {
                            player.Queue.Enqueue(tr);
                        }
                        player.QueueHistory.Clear();
                        return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"No songs currently playing." +
                            "\n\nDid you mean skip? -n" +
                            $"\n\nDid you mean {GlobalData.Config.DefaultPrefix}Stop?");
                    }
                    else
                    {
                        try
                        {
                            var currentTrack = player.Track;
                            await player.SkipAsync();
                            await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");

                            return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue);
                        }
                        catch (Exception ex)
                        {
                            return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
                        }
                    }
                }
                else   
                {
                    List <LavaTrack> Current = player.Queue.ToList();
                    List<LavaTrack> History = player.QueueHistory.ToList();
                    player.Queue.Clear();
                    player.QueueHistory.Clear();

                    LavaTrack pl1;
                    LavaTrack pl2;
                    try
                    {
                        pl1 = History.ElementAt(History.Count - 2);
                        pl2 = History.ElementAt(History.Count - 1);
                    }
                    catch
                    {
                        return await EmbedHandler.CreateErrorEmbed("Music Skip", "Unnable to skip track");
                    };
                  
                    Current.Insert(0, pl2);
                    Current.Insert(0, pl1);
                    History.RemoveAt(History.Count - 1);
                    History.RemoveAt(History.Count - 1);
                    foreach (var tr in History) { player.QueueHistory.Enqueue(tr); }
                    foreach (var tr in Current) { player.Queue.Enqueue(tr); }                    
                    await player.SkipAsync();

                    return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {pl2.Title}", Color.Blue);
                }
            }

            catch (Exception ex)    
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {

            if (!(args.Reason is TrackEndReason.Finished))
            {
                await (args.Player as MusicPlayer).TextChannel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{args.Player.Track.Title}]({args.Player.Track.Url})", Color.Blue));
                (args.Player as MusicPlayer).QueueHistory.Enqueue((args.Player as MusicPlayer).Track);
                return;
            }           
            var nextTrack = (args.Player as MusicPlayer).Queue.FirstOrDefault();
            if (nextTrack != null)
            {

                (args.Player as MusicPlayer).Queue.RemoveAt(0);
                await (args.Player as MusicPlayer).PlayAsync(nextTrack);
                await (args.Player as MusicPlayer).TextChannel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{nextTrack.Title}]({nextTrack.Url})", Color.Blue));
                return;
            }
        }

        public async Task<Embed> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                                                   
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }
                player.Queue.Clear();
                player.QueueHistory.Clear();
                await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
        }

        public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        {
            if (volume > 150 || volume <= 0)
            {
                return $"Volume must be between 1 and 150.";
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {                    
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}, what a bamboozle.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                { 
                    await player.ResumeAsync(); 
                }

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task Shaffle(IGuild guild)
        {
            var player = _lavaNode.GetPlayer(guild);
            player.Queue.Shuffle();
        }

        public   async Task UserLeft(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {

            if (user.IsBot)
                if (before.VoiceChannel.Users.Contains(user as SocketGuildUser)){
                    var player1 = _lavaNode.GetPlayer(before.VoiceChannel.Guild);
                    await _lavaNode.LeaveAsync(player1.VoiceChannel);
                };
                return;

            if (before.VoiceChannel is null)
            {
                return;
            }
            var player = _lavaNode.GetPlayer(before.VoiceChannel.Guild);
            if (player is null)
            {
                return;
            }
            if (after.VoiceChannel == null && player.PlayerState != PlayerState.Playing)
            {
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }          
        }
    }
}
