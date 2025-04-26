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
        public const int ItemsPerPage = 6;

        public SavedDescriptionsModel(ApplicationDbContext context, ILogger<SavedDescriptionsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<SavedJobDescription> SavedDescriptions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public async Task OnGetAsync()
        {
            try
            {
                var totalItems = await _context.SavedJobDescriptions.CountAsync();
                TotalPages = (int)Math.Ceiling(totalItems / (double)ItemsPerPage);
                
                // Ensure CurrentPage is within valid range
                if (CurrentPage < 1) CurrentPage = 1;
                if (CurrentPage > TotalPages) CurrentPage = TotalPages;

                SavedDescriptions = await _context.SavedJobDescriptions
                    .OrderByDescending(jd => jd.CreatedAt)
                    .Skip((CurrentPage - 1) * ItemsPerPage)
                    .Take(ItemsPerPage)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved job descriptions");
                ErrorMessage = "An error occurred while loading saved job descriptions.";
            }
        }
    }
} 