using Microsoft.EntityFrameworkCore;
using DevWebSecDemo.Entities;

namespace DevWebSecDemo.Providers
{
    /// <summary>
    /// DatabaseContext used for managing entities.
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        { }

        public DatabaseContext() : base()
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
