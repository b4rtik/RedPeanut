//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using Microsoft.EntityFrameworkCore;
using static RedPeanut.Models;

namespace RedPeanut
{
    public class RedPeanutDBContext : DbContext
    {
        public RedPeanutDBContext(DbContextOptions<RedPeanutDBContext> options) : base(options)
        {
        }

        public DbSet<WebResource> WebResources { get; set; }
        public DbSet<Listener> Listeners { get; set; }
        public DbSet<AgentInstance> Agents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebResource>().ToTable("WebResource");
            modelBuilder.Entity<Listener>().ToTable("Listener");
            modelBuilder.Entity<AgentInstance>().ToTable("AgentInstance");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Workspace/db/RedPeanut.db");
        }
    }
}
