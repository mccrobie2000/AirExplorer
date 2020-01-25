using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VirtualRadarServer.Models
{
    public class MyStandingDataContainer : StandingDataContainer
    {
        private string ConnectionString { get; set; }

        public MyStandingDataContainer(string connectionString) : base()
        {
            ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(ConnectionString);
            }
        }
    }
}
