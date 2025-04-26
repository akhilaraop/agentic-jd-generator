using Microsoft.AspNetCore.Mvc;
using MediatR;
using JobDescriptionAgent.Commands;
using JobDescriptionAgent.Models;

namespace JobDescriptionAgent.Controllers
{
    [ApiController]
    [Route("api/jd")]
    public class AgenticJobDescriptionController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AgenticJobDescriptionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "commands")]
        [ProducesResponseType(typeof(JDResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Generate([FromBody] JDRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.InitialInput))
            {
                return BadRequest("Initial input is required");
            }
            var response = await _mediator.Send(new GenerateJobDescriptionCommand { InitialInput = request.InitialInput });
            return Ok(response);
        }
    }
}

