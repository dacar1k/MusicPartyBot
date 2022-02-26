using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class PlayLists
    {
        private readonly DataBaseContext _contextDB;
        public PlayLists(DataBaseContext context)
        {
            _contextDB = context;
        }
        public async Task CreatePlaylist(ulong serverID, string name, string desc)
        {
            var playlist = await _contextDB.PlayLists.FindAsync(serverID);
            //if (access == null) access = false;
            if (playlist == null) _contextDB.PlayLists.Add(new PlayList { Name = name, ServerID = serverID, Description = desc, Tracks = new List<Track>() });
            await _contextDB.SaveChangesAsync();
        }

        public async Task<List<PlayList>> ShowPlayLists(ulong serverID)
        {   
            List<PlayList> list = await _contextDB.PlayLists.OrderBy(x => x.ServerID==serverID).ToListAsync();
            return list;
        }

        public async Task FindPlayList()
        {
            return;
        }

        public async Task<string> DeletePlaylist(ulong serverID, ulong PLID)
        {
            PlayList _pl = await _contextDB.PlayLists.Where(x => x.Id == PLID).FirstOrDefaultAsync();
            if (_pl != null)
            {
                _contextDB.PlayLists.Remove(_pl);
                await _contextDB.SaveChangesAsync();
                return "Successful deleted!";
            }
            else   return "An error was occurred";
        }

        public async Task AddTrack(ulong PLID, string url)
        {
            
        }
    }
}
