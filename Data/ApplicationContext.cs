using Emdad_Dashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Emdad_Dashboard.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> option) :base(option)
        {
        }

        public DbSet<Attendance> attendances { get; set; }
        public DbSet<KPI> kPIs { get; set; }
        public DbSet<Backup> Backups { get; set; }  
        public DbSet<SiteIssue> siteIssues { get; set; }  
        public DbSet<Vip> Vips { get; set; }  


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
