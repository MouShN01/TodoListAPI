using Microsoft.EntityFrameworkCore;
using TodoListAPI.Entities;
namespace TodoListAPI.Database
{
    public class ApplicationDbContext:DbContext
    {
        private readonly IConfiguration _configuration;
        public DbSet<Todo> Todos {get; set;}
        public DbSet<Account> Accounts { get; set;}
        public ApplicationDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(_configuration.GetConnectionString("Database"))
                .UseSnakeCaseNamingConvention();
        }
    }
}
