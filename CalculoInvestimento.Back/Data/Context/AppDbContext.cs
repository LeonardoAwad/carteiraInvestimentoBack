using CalculoInvestimento.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace CalculoInvestimento.Back.Data.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<User>? Users { get; set; }
        public DbSet<Investiments>? Investiments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("DataSource=app.db;Cache=Shared");
    }
}