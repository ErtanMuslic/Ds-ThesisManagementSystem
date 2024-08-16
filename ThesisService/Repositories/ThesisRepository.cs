using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ThesisService.Data;
using ThesisService.DTOs;
using ThesisService.Models;

namespace ThesisService.Repositories
{
    public class ThesisRepository : IThesisRepository
    {
        private readonly AppDbContext _context;

        public ThesisRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Thesis> CreateThesis(string professorId,string title, string description)
        {
            var newThesis = new Thesis
            {
             Title = title,
             Description = description,
             ProfessorID = professorId,
            };

            _context.Theses.Add(newThesis);
            await _context.SaveChangesAsync();
            return newThesis;
        }

        public void  DeleteThesis(int id)
        {
            var thesis =  _context.Theses.Find(id);
            if (thesis != null)
            {
                _context.Remove(thesis);
            }

        }

        public async Task<List<Thesis>> GetAllTheses()
        {
            return await _context.Theses.ToListAsync();
        }

        public  Thesis GetThesis(int ThesisId)
        {
            return  _context.Theses.Find(ThesisId);
        }

        public  IEnumerable<Thesis> GetThesisByProfessor(string professorId)
        {
            return _context.Theses
                .Where(c => c.ProfessorID == professorId);
        }

        public void UpdateThesis(int id,ThesisDTO thesisDTO)
        {
            var existingThesis = _context.Theses.FirstOrDefault(t => t.Id == id);
            if(existingThesis != null)
            {
                existingThesis.Title = thesisDTO.Title;
                existingThesis.Description = thesisDTO.Description;
                _context.Theses.Update(existingThesis);

            };

        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public bool ApplyForThesis(int thesisId, string studentId)
        {
            var existingThesisWithStudent = _context.Theses.FirstOrDefault(t => t.StudentID == studentId);
            if (existingThesisWithStudent != null)
            {
                existingThesisWithStudent.StudentID = "";
                _context.Theses.Update(existingThesisWithStudent);
            }

            var newThesis = _context.Theses.FirstOrDefault(t => t.Id == thesisId);
            if (newThesis != null)
            {
                newThesis.StudentID = studentId;
                _context.Theses.Update(newThesis);
                return SaveChanges();
            }
            else
            {
                return false;
            }

        }
    }
}
