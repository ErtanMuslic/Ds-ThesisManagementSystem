using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using ThesisService.DTOs;
using ThesisService.Repositories;

namespace ThesisService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public EventProcessor(IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.ThesisCreated:
                    ThesisCreated(message);
                    break;
                default:
                    break;
            }
        }

       

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Determining event...");

            var eventType = JsonSerializer.Deserialize<GenericEventDTO>(notificationMessage);
            Console.WriteLine("--> Event received: " + eventType.Event + " \nFrom this message: " + notificationMessage);

            switch (eventType.Event)
            {
                case "Thesis_Created":
                    Console.WriteLine("--> Thesis_Created event detected.");
                    return EventType.ThesisCreated;
                default:
                    Console.WriteLine("--> Could not determine event.");
                    return EventType.Undetermined;
            }
        }
        private async void ThesisCreated(string message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IThesisRepository>();

                var ThesisCreatedDTO = JsonSerializer.Deserialize<ThesisCreatedDTO>(message);

                if (VerifyToken(ThesisCreatedDTO.Token))
                {
                    try
                    {
                        var response = await repo.CreateThesis(ThesisCreatedDTO.Id, ThesisCreatedDTO.Title, ThesisCreatedDTO.Description);

                        if (response != null)
                        {
                            Console.WriteLine("-->successful.");
                        }
                        else
                        {
                            Console.WriteLine("-->failed...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--> Could not add Professor to DB {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"--> Token invalid");
                };
            }
        }
        private bool VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudiences = _configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                Console.WriteLine("--> The passed in token successfully authenticated.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("--> Error in validating token: " + ex.Message);
                return false;
            }
        }
    }

    enum EventType
    {
        ThesisCreated,
        Undetermined
    }
}
