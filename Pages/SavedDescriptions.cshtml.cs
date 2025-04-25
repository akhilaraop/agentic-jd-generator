using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobDescriptionAgent.Data;
using JobDescriptionAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace JobDescriptionAgent.Pages
{
    public class SavedDescriptionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SavedDescriptionsModel> _logger;

        public SavedDescriptionsModel(ApplicationDbContext context, ILogger<SavedDescriptionsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<SavedJobDescription> SavedDescriptions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                SavedDescriptions = await _context.SavedJobDescriptions
                    .OrderByDescending(jd => jd.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved job descriptions");
                ErrorMessage = "An error occurred while loading saved job descriptions.";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var description = await _context.SavedJobDescriptions.FindAsync(id);
                if (description == null)
                {
                    return NotFound();
                }

                _context.SavedJobDescriptions.Remove(description);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Job description deleted successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job description {Id}", id);
                ErrorMessage = "An error occurred while deleting the job description.";
                return Page();
            }
        }
    }
} 