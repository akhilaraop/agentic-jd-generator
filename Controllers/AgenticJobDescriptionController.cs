
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
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
            var result = await _workflowService.RunAgenticWorkflowAsync(request.InitialInput);
            return Ok(new { FinalJobDescription = result });
        }
    }
}
