using Core.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
            try
            {
                var conn = _configuration.GetConnectionString("DBConnection");
                Connection = new SqlConnection(conn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DBConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }

            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure the table name matches the expected name in the database
            modelBuilder.Entity<Division>().ToTable("Divisions");
        }

        public IDbConnection Connection { get; private set; }

        // Define DbSet properties for your entities
        public DbSet<Division> Divisions { get; set; }
    }
}
