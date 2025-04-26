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

        /// <summary>
        /// Retrieves all saved job descriptions in descending order of creation.
        /// </summary>
        /// <returns>A list of <see cref="SavedJobDescription"/> objects.</returns>
        [HttpGet]
        [ApiExplorerSettings(GroupName = "queries")]
        [ProducesResponseType(typeof(IEnumerable<SavedJobDescription>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var descriptions = await _mediator.Send(new GetSavedJobDescriptionsQuery());
            return Ok(descriptions);
        }

        /// <summary>
        /// Retrieves a saved job description by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the saved job description.</param>
        /// <returns>The <see cref="SavedJobDescription"/> if found; otherwise, NotFound.</returns>
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