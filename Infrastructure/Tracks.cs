using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Tracks
    {
        private readonly DataBaseContext _contextDB;
        public Tracks(DataBaseContext context)
        {
            _contextDB = context;
        }

        public async Task<List<Track>> GetTracks(ulong serverID, string name)
        {

            var pl = await _contextDB.PlayLists.Where(x => x.Name == name.ToLower() && x.ServerID == serverID).FirstOrDefaultAsync();
            //List<Track> tracks = await _contextDB.Tracks.Where(x => x.PlayListID == pl.Id).ToListAsync();
            List<Track> tracks = await _contextDB.Tracks.OrderBy(x => x.PlayListID == pl.Id).ToListAsync();
            return tracks;
        }

        public async Task AddTrack(string name, string title, string url)
        {
            var plID = await _contextDB.PlayLists.Where(x => x.Name == name.ToLower()).FirstOrDefaultAsync();
            _contextDB.Tracks.Add(new Track { Title = title, Link = url, PlayListID = plID.Id });
            await _contextDB.SaveChangesAsync();
        }

        public async Task RemoveTrack(ulong serverID, string name, string title)
        {
            var pl = await _contextDB.PlayLists.Where(x => x.Name == name.ToLower() && x.ServerID == serverID).FirstOrDefaultAsync();
            var track = await _contextDB.Tracks.Where(x => x.Title == title && x.PlayListID == pl.Id).FirstOrDefaultAsync();
            _contextDB.Tracks.Remove(track);
            await _contextDB.SaveChangesAsync();
        }
    }
}
