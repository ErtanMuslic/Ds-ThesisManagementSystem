using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProfessorService.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProfessorService.Models;
using ProfessorService.Data;
using ProfessorService.AsyncDataService;
using ProfesorService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ProfessorService.Repositories
{
    public class ProfessorRepository : IProfessorRepository
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Professor> _userManager;
        private readonly IConfiguration _config;
        private readonly IMessageBusClient _messageBusClient;

        public ProfessorRepository(AppDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<Professor> userManager,
            IConfiguration config,
            IMessageBusClient messageBusClient)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _config = config;
            _messageBusClient = messageBusClient;
        }

        public IEnumerable<Professor> GetAll()
        {
            return _context.Professors.ToList();
        }

        public Professor GetById(string id)
        {
            return _context.Professors.Find(id);
        }

        public async Task<string> Add(ProfessorDTO professor)
        {
            var newProfessor = new Professor
            {
                Name = professor.Name,
                Department = professor.Department,
                UserName = professor.Email,
                Email = professor.Email

            };

            var user = await _userManager.FindByEmailAsync(newProfessor.Email);
            if (user is not null) return "User registered already";

            var createUser = await _userManager.CreateAsync(newProfessor!, professor.Password);

            var checkAdmin = await _roleManager.FindByNameAsync("Admin");
            if (checkAdmin is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Professor" });
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });


                await _userManager.AddToRoleAsync(newProfessor, "Professor");
                await _userManager.AddToRoleAsync(newProfessor, "Admin");
                return "Account Created";

            }
            else
            {
                await _userManager.AddToRoleAsync(newProfessor, "Professor");
                return "Account Created";
            }
        }

        public async Task<string> LoginAccount(LoginDTO loginDTO)
        {
            if (loginDTO == null) return "Login container is empty";

            var getUser = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (getUser is null)
            {
                return "User not found";
            }

            var checkUserPassword = await _userManager.CheckPasswordAsync(getUser, loginDTO.Password);
            if (getUser is null)
            {
                return "Invalid email/Password";
            }

            var getUserRoles = await _userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Email, getUserRoles.ToList());
            string token = GenerateToken(userSession);
            return token;

        }

        public void Update(string id,UpdateDTO professor)
        {
            var existingProfessor = _context.Professors.FirstOrDefault(t => t.Id == id);
            if(existingProfessor != null)
            {
                existingProfessor.Name = professor.Name;
                existingProfessor.Email = professor.Email;
                existingProfessor.Department = professor.Department;
                _context.Professors.Update(existingProfessor);
            };



            
        }

        public void Delete(string id)
        {
            var professor = _context.Professors.Find(id);
            if (professor != null)
            {
                _context.Professors.Remove(professor);
            }
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }


        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };
            userClaims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var audiences = _config.GetSection("Jwt:Audiences").Get<List<string>>();
            userClaims.AddRange(audiences.Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience)));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public bool CreateThesis(string token,string title,string description)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            string email = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

            string id = _context.Professors
                .Where(t => t.Email == email)
                .Select(t => t.Id)
                .FirstOrDefault();

            try
            {
                CreateThesisDTO createThesisDTO = new CreateThesisDTO
                {
                    Id = id,
                    Title = title,
                    Description = description,
                    Event = "Thesis_Created",
                    Token = token
                };

                _messageBusClient.CreateNewThesis(createThesisDTO);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("--> Failed to send Create_Thesis to RabbitMQ Message Bus: " + ex.Message);
                return false;
            }
        }

        public record UserSession(string Email, List<string> Roles);
    }
}
