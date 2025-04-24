using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobDescriptionAgent.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobDescriptionAgent.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAgenticWorkflowService _workflowService;
        private readonly ILogger<IndexModel> _logger;
        
        [BindProperty]
        [Required(ErrorMessage = "Please provide job requirements.")]
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(JobDescription))
                {
                    ModelState.AddModelError(string.Empty, "Please provide job requirements.");
                    return Page();
                }

                var (description, stages) = await _workflowService.RunAsync(JobDescription);
                
                if (string.IsNullOrEmpty(description))
                {
                    ErrorMessage = "Failed to generate job description. Please try again with more specific requirements.";
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

            return Page();
        }
    }
} 