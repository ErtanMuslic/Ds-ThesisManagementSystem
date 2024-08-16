using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThesisService.DTOs;
using ThesisService.Models;
using ThesisService.Repositories;

namespace ThesisService.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ThesisController : ControllerBase
    {
        private readonly IThesisRepository _thesisRepository;

        public ThesisController(IThesisRepository thesisRepository) 
        {
            _thesisRepository = thesisRepository;
        }

        [HttpGet("professor/{professorId}")]
        public  ActionResult<IEnumerable<Thesis>> GetProfessorThesis(string  professorId) 
        {
            var theses =  _thesisRepository.GetThesisByProfessor(professorId);
            return Ok(theses);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetTheses()
        {
            var theses = await _thesisRepository.GetAllTheses();
            return Ok(theses);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Professor")]
        public ActionResult<Thesis> GetThesisById(int id)
        {
            var theses = _thesisRepository.GetThesis(id);
            if (theses == null)
            {
                return NotFound();
            }
            return Ok(theses);
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateThesis(string professorId, ThesisDTO thesisDTO)
        {
            if (thesisDTO == null)
            {
                return BadRequest("Bad Request");
            }

            try
            {
                var createdThesis = await _thesisRepository.CreateThesis(professorId,thesisDTO.Title,thesisDTO.Description);
                return CreatedAtAction(nameof(GetThesisById), new { id = createdThesis.Id }, createdThesis);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update")]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateThesis(int id, [FromBody] ThesisDTO updatedThesis)
        {
            var thesis = _thesisRepository.GetThesis(id);
            if (thesis == null)
            {
                return NotFound();
            }

            _thesisRepository.UpdateThesis(id,updatedThesis);
            _thesisRepository.SaveChanges();
            return NoContent();
        }

        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteThesis(int id)
        {
            var thesis = _thesisRepository.GetThesis(id);
            if (thesis == null)
            {
                return NotFound();
            }

            _thesisRepository.DeleteThesis(id);
            _thesisRepository.SaveChanges();
            return NoContent();
        }
    }
}
