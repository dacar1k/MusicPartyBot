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

        public async Task<List<Track>> GetTracks(ulong serverID, ulong id)
        {

            var pl = await _contextDB.PlayLists.Where(x => x.Id == id & x.ServerID == serverID).FirstOrDefaultAsync();
            List<Track> tracks = await _contextDB.Tracks.Where(x => x.PlayListID == pl.Id).ToListAsync();
            return tracks;
        }

        public async Task AddTrack(ulong serverID, ulong id, string title, string url)
        {
            var plID = await _contextDB.PlayLists.Where(x => x.Id == id & x.ServerID == serverID).FirstOrDefaultAsync();
            _contextDB.Tracks.Add(new Track { Title = title, Link = url, PlayListID = plID.Id });
            await _contextDB.SaveChangesAsync();
        }

        public async Task RemoveTrack(ulong serverID,ulong id, string title)
        {
            var plID = await _contextDB.PlayLists.Where(x => x.Id == id & x.ServerID == serverID).FirstOrDefaultAsync();
            var track = await _contextDB.Tracks.Where(x => x.Title.Trim() == title.Trim() && x.PlayListID == plID.Id).FirstOrDefaultAsync();
            _contextDB.Tracks.Remove(track);
            await _contextDB.SaveChangesAsync();
        }
    }
}
