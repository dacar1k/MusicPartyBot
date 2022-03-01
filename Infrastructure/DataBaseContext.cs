using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections;
using System.Collections.Generic;

namespace Infrastructure
{
        public class DataBaseContext : DbContext
        {
            public DbSet<Server> Servers { get; set; }
            public DbSet<PlayList> PlayLists { get; set; }
            public DbSet<Track> Tracks { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseMySql("server=localhost;user=root;database=MusicStreaming;port=3306;Connect Timeout=5;");
    }

    public class Server
        {
            public ulong Id { get; set; }
            public string Prefix { get; set; }
            public string GameStatus { get; set; }
        }

        public class PlayList
        {            
            public ulong Id { get; set; }
            public ulong ServerID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            //public bool Access { get ; set; }
            public ICollection<Track> Tracks { get; set; }
            public PlayList()
            {
                Tracks = new List<Track>();
            }
        }
        
        public class Track
        {
            public ulong Id { get; set; }
            public ulong PlayListID { get; set; }
            public string Title { get; set; } 
            public string Link { get; set; }             
        }
}
