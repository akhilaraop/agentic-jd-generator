using MediatR;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Services;

namespace JobDescriptionAgent.Commands
{
    /// <summary>
    /// Command to generate a job description from initial input.
    /// </summary>
    /// <remarks>
    /// Input: InitialInput (string) - the requirements for the job description.
    /// Output: JDResponse - the generated job description and related information.
    /// </remarks>
    public class GenerateJobDescriptionCommand : IRequest<JDResponse>
    {
        public string InitialInput { get; set; } = string.Empty;
        public string ModelKey { get; set; } = "llama3-8b-8192";
    }

    /// <summary>
    /// Handler for <see cref="GenerateJobDescriptionCommand"/>. Executes the job description generation workflow.
    /// </summary>
    public class GenerateJobDescriptionCommandHandler : IRequestHandler<GenerateJobDescriptionCommand, JDResponse>
    {
        private readonly IAgenticWorkflowService _workflowService;
        public GenerateJobDescriptionCommandHandler(IAgenticWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }
        /// <summary>
        /// Handles the command to generate a job description.
        /// </summary>
        /// <param name="request">The command containing the initial input.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="JDResponse"/> with the generated job description.</returns>
        public async Task<JDResponse> Handle(GenerateJobDescriptionCommand request, CancellationToken cancellationToken)
        {
            var (description, stages) = await _workflowService.RunAsync(request.InitialInput, request.ModelKey);
            return new JDResponse
            {
                FinalJobDescription = description,
                // Optionally map other fields from stages if needed
            };
        }
    }
} 