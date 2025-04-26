using MediatR;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Services;

namespace JobDescriptionAgent.Commands
{
    public class GenerateJobDescriptionCommand : IRequest<JDResponse>
    {
        public string InitialInput { get; set; } = string.Empty;
    }

    public class GenerateJobDescriptionCommandHandler : IRequestHandler<GenerateJobDescriptionCommand, JDResponse>
    {
        private readonly IAgenticWorkflowService _workflowService;
        public GenerateJobDescriptionCommandHandler(IAgenticWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }
        public async Task<JDResponse> Handle(GenerateJobDescriptionCommand request, CancellationToken cancellationToken)
        {
            var (description, stages) = await _workflowService.RunAsync(request.InitialInput);
            return new JDResponse
            {
                FinalJobDescription = description,
                // Optionally map other fields from stages if needed
            };
        }
    }
} 