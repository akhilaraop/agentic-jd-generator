using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobDescriptionAgent.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace JobDescriptionAgent.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAgenticWorkflowService _workflowService;
        private readonly ILogger<IndexModel> _logger;
        
        [BindProperty]
        public string? JobDescription { get; set; }
        
        public string? GeneratedDescription { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string>? Stages { get; set; }

        public IndexModel(IAgenticWorkflowService workflowService, ILogger<IndexModel> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(JobDescription))
            {
                return Page();
            }

            try
            {
                var (description, stages) = await _workflowService.RunAsync(JobDescription);
                GeneratedDescription = description;
                Stages = stages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating job description");
                ErrorMessage = "An error occurred while generating the job description. Please try again.";
            }

            return Page();
        }
    }
} 