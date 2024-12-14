using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DBConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        // Define DbSet properties for your entities
        // public DbSet<YourEntity> YourEntities { get; set; }
    }
}
