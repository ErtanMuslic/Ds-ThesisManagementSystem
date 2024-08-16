using ProfesorService.DTOs;
using ProfessorService.DTOs;
using ProfessorService.Models;

namespace ProfessorService.Repositories
{
    public interface IProfessorRepository
    {
        IEnumerable<Professor> GetAll();
        Professor GetById(string id);
        Task<string> Add(ProfessorDTO student);
        void Update(string id,UpdateDTO student);
        void Delete(string id);

        bool CreateThesis(string token,string title,string description);
        Task<string> LoginAccount(LoginDTO login);
        bool SaveChanges();
    }
}
