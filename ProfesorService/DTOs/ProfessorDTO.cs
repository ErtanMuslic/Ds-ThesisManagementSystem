namespace ProfessorService.DTOs
{
    public class ProfessorDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;

        public string confirmPassword { get; set; } = string.Empty;

        public string Department { get; set; }

    }
}
