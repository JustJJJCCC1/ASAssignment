using Microsoft.EntityFrameworkCore;

namespace ASAssignment1.Model
{
	public class AuthDbContext : DbContext
	{
		private readonly IConfiguration _configuration;

		public AuthDbContext(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string? connectionString = _configuration.GetConnectionString("AuthConnectionString");
			optionsBuilder.UseSqlServer(connectionString);
		}

		// Define DbSets for your custom tables
		public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
