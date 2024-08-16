using Microsoft.EntityFrameworkCore;
using ThesisService.Models;

namespace ThesisService.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Thesis> Theses { get; set; }

        //public DbSet<Professor> Professors { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Professor>()
        //         .HasMany(p => p.Theses)
        //         .WithOne(p => p.Professor!)
        //         .HasForeignKey(p => p.ProfessorID);

        //    modelBuilder.Entity<Thesis>()
        //        .HasOne(p => p.Professor)
        //        .WithMany(p => p.Theses);
        //}
    }


}
