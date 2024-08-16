using ThesisService.DTOs;
using ThesisService.Models;

namespace ThesisService.Repositories
{
    public interface IThesisRepository
    {
        Task<Thesis> CreateThesis(string professorId,string title,string description);
        Thesis GetThesis(int ThesisId);

        IEnumerable<Thesis> GetThesisByProfessor(string professorId);
        Task<List<Thesis>> GetAllTheses();
        void UpdateThesis(int id, ThesisDTO thesisDTO);
        void DeleteThesis(int id);
        bool ApplyForThesis(int thesisId, string StudentId);
        bool SaveChanges();

    }
}
