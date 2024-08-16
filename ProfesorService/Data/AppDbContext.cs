using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProfessorService.Models;

namespace ProfessorService.Data
{
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<Professor>(options)
    {
        public DbSet<Professor> Professors { get; set; }
    }
}
