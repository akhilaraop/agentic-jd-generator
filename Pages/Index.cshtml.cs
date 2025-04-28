using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobDescriptionAgent.Services;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JobDescriptionAgent.Pages
{
    public class MainModel : PageModel
    {
        private readonly IAgenticWorkflowService _workflowService;
        private readonly ILogger<MainModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IDocumentProcessor _documentProcessor;
        private readonly IQueryProcessor _queryProcessor;
        public const int ItemsPerPage = 6;

        [BindProperty]
        [Required(ErrorMessage = "Please provide job requirements.")]
        public string? JobDescription { get; set; }
        [BindProperty]
        public string SelectedModel { get; set; } = "llama3-8b-8192";
        public string? GeneratedDescription { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string>? Stages { get; set; }

        public List<SavedJobDescription> SavedDescriptions { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public MainModel(
            IAgenticWorkflowService workflowService,
            ILogger<MainModel> logger,
            ApplicationDbContext context,
            IDocumentProcessor documentProcessor,
            IQueryProcessor queryProcessor)
        {
            _workflowService = workflowService;
            _logger = logger;
            _context = context;
            _documentProcessor = documentProcessor;
            _queryProcessor = queryProcessor;
        }

        public async Task OnGetAsync()
        {
            // Load Saved Descriptions
            var totalItems = await _context.SavedJobDescriptions.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)ItemsPerPage);
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            SavedDescriptions = await _context.SavedJobDescriptions
                .OrderByDescending(jd => jd.CreatedAt)
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(JobDescription))
                {
                    ModelState.AddModelError(string.Empty, "Please provide job requirements.");
                    await OnGetAsync();
                    return Page();
                }
                var (description, stages) = await _workflowService.RunAsync(JobDescription, SelectedModel);
                if (string.IsNullOrEmpty(description))
                {
                    ErrorMessage = "Failed to generate job description. Please try again with more specific requirements.";
                    await OnGetAsync();
                    return Page();
                }

                GeneratedDescription = description;
                Stages = stages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating job description");
                ErrorMessage = "An error occurred while generating the job description. Please try again.";
            }
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync(string title, string description, string initialInput)
        {
            try
            {
                var savedJobDescription = new SavedJobDescription
                {
                    Title = title,
                    Description = description,
                    InitialInput = initialInput,
                    CreatedAt = DateTime.UtcNow,
                    Stages = Stages ?? new Dictionary<string, string>()
                };

                _context.SavedJobDescriptions.Add(savedJobDescription);
                await _context.SaveChangesAsync();
                // Asynchronously process for embeddings
                _ = Task.Run(() => _documentProcessor.ProcessAsync(savedJobDescription));
                TempData["SuccessMessage"] = "Job description saved successfully!";
                await OnGetAsync();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving job description");
                ErrorMessage = "An error occurred while saving the job description. Please try again.";
                await OnGetAsync();
                return Page();
            }
        }
    }
} 