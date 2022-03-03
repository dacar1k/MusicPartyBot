using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Servers
    {
        private readonly DataBaseContext _context;
        public Servers(DataBaseContext context)
        {
            _context = context;
        }

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null) _context.Add(new Server { Id = id, Prefix = prefix });
            else server.Prefix = prefix;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            var prefix = await _context.Servers.Where(x => x.Id == id).Select(x => x.Prefix).FirstOrDefaultAsync();
            return await Task.FromResult(prefix);
        }

        public async Task SetStatus(ulong id, string status)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server != null) {server.GameStatus = status; await _context.SaveChangesAsync(); }            
        }
    }
}
