using Grpc.Core;
using ThesisService.Protos;
using ThesisService.Repositories;

namespace ThesisService.SyncDataServices
{
    public class GrpcThesisService : Protos.ParticipateService.ParticipateServiceBase
    {
        private readonly ILogger<GrpcThesisService> _logger;
        private readonly IThesisRepository _thesisRepository;

        public GrpcThesisService(ILogger<GrpcThesisService> logger,IThesisRepository thesisRepository)
        {
            _logger = logger;
            _thesisRepository = thesisRepository;
        }
        public override Task<PartitipationResponse> Participate(Thesis request, ServerCallContext context)
        {
            bool approved = false;
            _logger.LogInformation($"Received request for Thesis ID: {request.ThesisId}, {request.StudentId}");
            // Business logic to evaluate the participation request
            if(request.ThesisId > 0 && !string.IsNullOrEmpty(request.StudentId))
            {
                 approved = _thesisRepository.ApplyForThesis(request.ThesisId, request.StudentId);


            }

            //bool approved = request.ThesisId > 0 && !string.IsNullOrEmpty(request.StudentId);

            _logger.LogInformation($"{approved}");



            return Task.FromResult(new PartitipationResponse
            {
                Approved = approved
            });
        }
    }
}
