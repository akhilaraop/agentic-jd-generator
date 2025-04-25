using Microsoft.AspNetCore.Mvc;
using JobDescriptionAgent.Data;
using JobDescriptionAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace JobDescriptionAgent.Controllers
{
    [ApiController]
    [Route("api/saved-descriptions")]
    public class SavedJobDescriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SavedJobDescriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SavedJobDescription>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var descriptions = await _context.SavedJobDescriptions
                .OrderByDescending(jd => jd.CreatedAt)
                .ToListAsync();

            return Ok(descriptions);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SavedJobDescription), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var description = await _context.SavedJobDescriptions.FindAsync(id);

            if (description == null)
            {
                return NotFound();
            }

            return Ok(description);
        }
    }
} 