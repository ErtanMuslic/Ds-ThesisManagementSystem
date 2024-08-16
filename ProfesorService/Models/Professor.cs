using Microsoft.AspNetCore.Identity;

namespace ProfessorService.Models
{
    public class Professor : IdentityUser
    {
        public string? Name { get; set; }
        public string? Department { get; set; }
    }
}
