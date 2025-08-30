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
            //base.OnModelCreating(modelBuilder);

            //// Ensure the table name matches the expected name in the database
            //modelBuilder.Entity<Division>().ToTable("Divisions");
        }

        public IDbConnection Connection { get; private set; }

        // Define DbSet properties for your entities
        public DbSet<Division> Divisions { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Village> Villages { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<InstitutionGradeSection> InstitutionGradeSections { get; set; }
        public DbSet<InstitutionPartner> InstitutionPartners { get; set; }
        public DbSet<InstitutionProject> InstitutionProjects { get; set; }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<MenuPermission> MenuPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectProgram> ProjectPrograms { get; set; }
        public DbSet<ProjectInstitutionType> ProjectInstitutionTypes { get; set; }
        public DbSet<ProjectState> ProjectStates { get; set; }
        public DbSet<UserProgram> UserPrograms { get; set; }
        public DbSet<PeopleInstitution> PeopleInstitutions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentFamilyDetail> StudentFamilyDetails { get; set; }
        public DbSet<StudentHealth> StudentHealths { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<StudentBaselineDetail> StudentBaselineDetails { get; set; }
        public DbSet<StudentMainstream> StudentMainstreams { get; set; }
        public DbSet<StudentFollowup> StudentFollowups { get; set; }
        public DbSet<ThemeActivity> ThemeActivities { get; set; }
        public DbSet<StudentAttendance> StudentAttendances { get; set; }
        public DbSet<StudentTrio> StudentTrios { get; set; }

    }
}
