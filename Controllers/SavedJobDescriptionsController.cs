using Microsoft.AspNetCore.Mvc;
using MediatR;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Queries;

namespace JobDescriptionAgent.Controllers
{
    [ApiController]
    [Route("api/saved-descriptions")]
    public class SavedJobDescriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SavedJobDescriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ApiExplorerSettings(GroupName = "queries")]
        [ProducesResponseType(typeof(IEnumerable<SavedJobDescription>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var descriptions = await _mediator.Send(new GetSavedJobDescriptionsQuery());
            return Ok(descriptions);
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(GroupName = "queries")]
        [ProducesResponseType(typeof(SavedJobDescription), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var descriptions = await _mediator.Send(new GetSavedJobDescriptionsQuery());
            var description = descriptions.FirstOrDefault(d => d.Id == id);
            if (description == null)
            {
                return NotFound();
            }
            return Ok(description);
        }
    }
} 