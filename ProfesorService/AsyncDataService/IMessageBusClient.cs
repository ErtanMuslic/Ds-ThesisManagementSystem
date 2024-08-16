using ProfesorService.DTOs;
using ProfessorService.DTOs;

namespace ProfessorService.AsyncDataService
{
    public interface IMessageBusClient
    {
        void CreateNewThesis(CreateThesisDTO createThesisDTO);
    }
}
