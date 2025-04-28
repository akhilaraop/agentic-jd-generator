using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobDescriptionAgent.Data;
using JobDescriptionAgent.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using JobDescriptionAgent.Services;

namespace JobDescriptionAgent.Pages
{
    public class SavedDescriptionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SavedDescriptionsModel> _logger;
        private readonly IQueryProcessor _queryProcessor;
        public const int ItemsPerPage = 6;
        private readonly string queryApiUr = "http://embeddings-query-backend:9061/api/query/id"; // Updated for Docker Compose networking and new port

        public SavedDescriptionsModel(ApplicationDbContext context, ILogger<SavedDescriptionsModel> logger, IQueryProcessor queryProcessor)
        {
            _context = context;
            _logger = logger;
            _queryProcessor = queryProcessor;
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

        // Handler for AJAX job description chat queries
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostQueryAsync()
        {
            
            try
            {
                _logger.LogInformation("Received query request");
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(body))
                    return BadRequest("Empty request body");

                var req = System.Text.Json.JsonSerializer.Deserialize<QueryRequest>(body);
                if (req is null || string.IsNullOrWhiteSpace(req.job_id) || string.IsNullOrWhiteSpace(req.query))
                    return BadRequest("Invalid request data");

                var apiContent = await _queryProcessor.QueryAsync(req.job_id, req.query);
                return Content(apiContent, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying query to backend");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class QueryRequest
        {
            public string job_id { get; set; }
            public string query { get; set; }
        }
    }
} 