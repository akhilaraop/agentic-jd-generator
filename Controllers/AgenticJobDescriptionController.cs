using Microsoft.AspNetCore.Mvc;
using JobDescriptionAgent.Services;
using JobDescriptionAgent.Models;

namespace JobDescriptionAgent.Controllers
{
    [ApiController]
    [Route("api/jd")]
    public class AgenticJobDescriptionController : ControllerBase
    {
        private readonly JDOrchestrator _orchestrator;

        public AgenticJobDescriptionController(JDOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(JDResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Generate([FromBody] JDRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.InitialInput))
            {
                return BadRequest("Initial input is required");
            }

            var (jd, complianceReview) = await _orchestrator.RunAsync(request.InitialInput);

            if (jd.StartsWith("Clarifier Agent needs more information"))
            {
                return Ok(new JDResponse
                {
                    FinalJobDescription = jd,
                    ComplianceReview = ""
                });
            }

            return Ok(new JDResponse
            {
                FinalJobDescription = jd,
                ComplianceReview = complianceReview
            });
        }
    }
}

