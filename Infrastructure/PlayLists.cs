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

        public async Task<bool> CheckIfExists(ulong serverID, string name)
        {
            try
            {
                var pl = _contextDB.PlayLists.Where(x => x.ServerID == serverID & x.Name == name.ToLower()).FirstOrDefault();
                if (pl != null) return true; else return false;
            }
            catch
            {
                return false;
            }           
        }

        public async Task CreatePlaylist(ulong serverID, string name, string desc)
        {
            _contextDB.PlayLists.Add(new PlayList { Name = name.ToLower(), ServerID = serverID, Description = desc, Tracks = new List<Track>() });
            await _contextDB.SaveChangesAsync();
        }

        public async Task<List<PlayList>> ShowPlayLists(ulong serverID)
        {   
            List<PlayList> list = await _contextDB.PlayLists.Where(x => x.ServerID==serverID).ToListAsync();
            if (list == null)
            {
                return null;
            }
            else return list;
        }
        public async Task<string> GetName(ulong serverID, ulong id)
        {
            var pl = await _contextDB.PlayLists.Where(x => x.Id == id & x.ServerID == serverID).FirstOrDefaultAsync();
            return pl.Name;
        }
        public async Task<string> DeletePlaylist(ulong serverID, ulong ID)
        {
            PlayList _pl = await _contextDB.PlayLists.Where(x => x.Id == ID & x.ServerID == serverID).FirstOrDefaultAsync();
            if (_pl  !=  null)
            {
                var tracks = _contextDB.PlayLists.Where(x => x.Id == ID).Include(x => x.Tracks).First();
                _contextDB.Remove(tracks);
                await _contextDB.SaveChangesAsync();
                return "Successful deleted!";
            }
            else   return "An error was occurred";            
        }
    }
}
