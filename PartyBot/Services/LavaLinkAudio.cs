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
using Victoria.Responses.Rest;
using Infrastructure;
using System.Collections.Generic;
using MusicStreaming.DataStructs;

namespace MusicStreaming.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly Tracks _tracks;
        private readonly LavaNode _lavaNode;

        private readonly List<queueManager> qeuemanager = new List<queueManager>();

        public LavaLinkAudio(LavaNode lavaNode, Tracks tracks)
        {
            _lavaNode = lavaNode;
            _tracks = tracks;
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
                qeuemanager.Add(new queueManager(guild.Id, null, 0, 0));
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        //public async Task<Embed> LoadPL(SocketGuildUser user, IGuild guild, string name) //сделать 
        //{            
        //    if (user.VoiceChannel == null)
        //    {
        //        return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
        //    }
        //    var tracks = await _tracks.GetTracks(guild.Id, name);
        //    var player = _lavaNode.GetPlayer(guild);
        //    foreach(var track in tracks)
        //    {

        //    }
        //}

        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }
            if (!_lavaNode.HasPlayer(guild))
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                qeuemanager.Add(new queueManager(guild.Id, new List<LavaTrack>(), 0, 0));
                await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }

            try
            {
                var player = _lavaNode.GetPlayer(guild);

                LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);


                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                }

                track = search.Tracks.FirstOrDefault();
                var q = qeuemanager.Where(x => x.serverID == guild.Id).FirstOrDefault();
                if (q.TrackLink == null) q.TrackLink = new List<LavaTrack>();
                if (q == null) qeuemanager.Add(new queueManager(guild.Id, new List<LavaTrack>(), 0, 0)); //нахера - не знаю

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    
                    //int tnumb = 0;
                    q.TrackLink.Add(track);
                    //player.Queue.Enqueue(track); 
                    await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue.");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue);
                }
                q.TrackLink.Add(track);
                await player.PlayAsync(track);
                await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue);
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
                    var q = qeuemanager.Where(x => x.serverID == guild.Id).FirstOrDefault();
                    qeuemanager.Remove(q);
                    await player.StopAsync();
                }

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await LoggingService.LogInformationAsync("Music", $"Bot has left.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing moosik.", Color.Blue);
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
                var queue = qeuemanager.Where(x => x.serverID == guild.Id).FirstOrDefault();
                
                if (queue == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
                if (player.PlayerState is PlayerState.Playing)
                {
                    if (queue.TrackLink.Count < 1 && player.Track != null)
                    //if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.", Color.Blue);
                    }
                    else
                    {
                        var trackNum = 1;
                        //foreach (LavaTrack track in player.Queue)
                        foreach (LavaTrack track in queue.TrackLink) //wefgrhtyj тут исправить срочно1
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
                            trackNum++;
                        }
                        return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
                    }
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error.");
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
                var queue = qeuemanager.Where(x => x.serverID == guild.Id).FirstOrDefault();
                if (player == null)
                {
                    return await EmbedHandler.CreateErrorEmbed(null, $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
                }
                //else if (player.Queue.Count <= id)
                else if (queue.TrackLink.Count <= id)
                {
                    return await EmbedHandler.CreateErrorEmbed(null, $"Track with id {id} doesn't exist in queue");
                }
                //player.Queue.RemoveAt(id);
                queue.TrackLink.RemoveAt(id - 1);
                return await EmbedHandler.CreateBasicEmbed(null, $"Track with id {id} was removed from queue", Color.Blue);
            } 

            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }
        }

        public async Task<Embed> SaveAsync(ulong serverID, IGuild guild)  //мое новое
        {
            var player = _lavaNode.GetPlayer(guild);
            try
            {
                if (player.Queue != null)
                {
                    return await EmbedHandler.CreateBasicEmbed("Music, List", "Playlist saved succesfull!", Color.Blue);
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Can't save empty queue.");
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }
        }

        public async Task<Embed> SkipTrackAsync(IGuild guild) //переделать
        {
            try
            {
                
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
                
                var queue =  qeuemanager.Where(x => x.serverID == guild.Id).FirstOrDefault();
                queue.inc++;
                var currentTrack = player.Track;
                var pos = queue.TrackLink.IndexOf(currentTrack) + 2;
                if (queue.TrackLink.Count < pos)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing." +
                        $"\n\nDid you mean {GlobalData.Config.DefaultPrefix}Stop?");
                }
                else
                {
                    try
                    {

                        //доработать
                        //var currentTrack = player.Track;
                        //await player.SkipAsync();
                        //int pos = queue.TrackLink.IndexOf(currentTrack);
                        //int pos = queue.TrackLink.IndexOf(currentTrack) + 1;

                        //if (pos > queue.TrackLink.Count())
                        //return await EmbedHandler.CreateBasicEmbed("Playlist, Song", "Nothing to play next \n Playback Finished.", Color.Blue);

                        await player.PlayAsync(queue.TrackLink.ElementAt(queue.TrackLink.IndexOf(currentTrack) + 1));
                        await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
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

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            var serverID = args.Player.VoiceChannel.GuildId;
            var queue = qeuemanager.Where(x => x.serverID == serverID).FirstOrDefault();
            var currentTrack = args.Player.Track;

            queue.inc++;

            if (queue.inc >= queue.TrackLink.Count)
            {
                await args.Player.TextChannel.SendMessageAsync("Nothing to play");
                return;
            }

            //if (!args.Reason.ShouldPlayNext()) //срабатывает это 
            //{
            //    return;
            //}

            //if (!args.Player.Queue.TryDequeue(out var queueable))
            //{
            //    await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
            //    return;
            //}

            //if (!(queueable is LavaTrack track))
            //{
            //    await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            //    return;
            //}

            //await args.Player.PlayAsync(track);
            //await args.Player.TextChannel.SendMessageAsync(
            //    embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue));
        }

        public async Task LoopAsync(IGuild guild)
        {
            var player =  _lavaNode.GetPlayer(guild);            
        }
    }
}
