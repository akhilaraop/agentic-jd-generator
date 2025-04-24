
using Microsoft.AspNetCore.Mvc;
using JobDescriptionAgent.Services;
using JobDescriptionAgent.Models;

namespace JobDescriptionAgent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenticJobDescriptionController : ControllerBase
    {
        private readonly AgenticWorkflowService _workflowService;

        public AgenticJobDescriptionController(AgenticWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] JDRequest request)
        {
            var (jd, complianceReview) = await _workflowService.RunAgenticWorkflowAsync(request.InitialInput);

    if (jd.StartsWith("Clarifier Agent needs more information"))
    {
        // return early if clarification is needed
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
