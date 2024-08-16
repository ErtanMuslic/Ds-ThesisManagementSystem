using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfesorService.DTOs;
using ProfessorService.DTOs;
using ProfessorService.Models;
using ProfessorService.Repositories;

namespace ProfessorService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly IProfessorRepository _professorRepository;

        public ProfessorController(IProfessorRepository professorRepository)
        {
            _professorRepository = professorRepository;
        }


        [HttpGet("Authorize")]
        public IResult Auth()
        {
            try
            {
                if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
                {
                    Console.WriteLine("--> Nginx authorization FAILED... controller");
                    return Results.Unauthorized();
                }

                Console.WriteLine("--> Nginx authorization successful... controller");
                return Results.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Results.Problem("Internal Server Error");
            }
        }

        [HttpGet("All")]
        public ActionResult<IEnumerable<Professor>> GetProfessors()
        {
            var professors = _professorRepository.GetAll();
            return Ok(professors);
        }

        [HttpGet("Test")]
        public string Test()
        {
            return "Radi";
        }

        [HttpGet("{id}")]
        public ActionResult<Professor> GetProfessor(string id)
        {
            var professor = _professorRepository.GetById(id);
            if (professor == null)
            {
                return NotFound();
            }
            return Ok(professor);
        }


        [HttpPost("Create")]
        public async Task<ActionResult<Professor>> CreateProfessor([FromBody] ProfessorDTO professor)
        {
            var result = await _professorRepository.Add(professor);
            _professorRepository.SaveChanges();
            return Ok(result);
        }


        [HttpPost("CreateThesis")]
        [Authorize(Roles = "Professor")]
        public IActionResult CreateThesis([FromBody] ThesisDTO thesisDTO)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var response = _professorRepository.CreateThesis(token, thesisDTO.title, thesisDTO.description);
            return response ? Ok("Event to add thesis with title:" + thesisDTO.title + " has been pushed to the RabbitMQ queue.") : NotFound("Failed to create thesis from professor controller...");
        }

        [HttpPost("Login")]
        public async Task<string> Login(LoginDTO loginDTO)
        {
            var result = await _professorRepository.LoginAccount(loginDTO);
            return result;
        }

        [HttpPut("Update")]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateProfessor(string id, [FromBody] UpdateDTO updatedProfessor)
        {
            var professor = _professorRepository.GetById(id);
            if (professor == null)
            {
                return NotFound("User not found");
            }

            _professorRepository.Update(id,updatedProfessor);
            _professorRepository.SaveChanges();
            return Ok("User updated successfully");
        }

        


        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteProfessor(string id)
        {
            var professor = _professorRepository.GetById(id);
            if (professor == null)
            {
                return NotFound("User not found");
            }

            _professorRepository.Delete(id);
            _professorRepository.SaveChanges();
            return Ok("User Deleted");
        }
    }
}
