using Lyrica.Data.Guilds;
using Lyrica.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Lyrica.Data
{
    public class LyricaContext : DbContext
    {
        public LyricaContext(DbContextOptions<LyricaContext> options) : base(options) { }

        public DbSet<Guild> Guilds { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LyricaContext).Assembly);
        }
    }
}