using Microsoft.EntityFrameworkCore;
using Porumb_Denisa_Lab4.Models;

namespace Porumb_Denisa_Lab4.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}
