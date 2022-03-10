using System;
using System.Collections.Generic;
using System.Text;
using Victoria;

namespace MusicStreaming.DataStructs
{
    public class queueManager
    {
        public ulong serverID { get; set; }
        
        public int inc { get; set; }

        public List<LavaTrack> TrackLink { get; set; }

        public int Loop { get; set; }

        public queueManager(ulong server, List<LavaTrack> url, int loop, int Inc)
        {
            serverID = server;
            TrackLink = url;
            Loop = loop;
            inc = Inc;
        }
    }
}
