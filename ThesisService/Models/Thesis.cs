namespace ThesisService.Models
{
    public class Thesis
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string ProfessorID { get; set; }

        public string StudentID { get; set; } = String.Empty;

        //public Professor? Professor { get; set; }
    }
}
