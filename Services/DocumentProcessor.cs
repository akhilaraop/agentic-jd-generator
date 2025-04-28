using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using JobDescriptionAgent.Models;
using Microsoft.Extensions.Logging;

namespace JobDescriptionAgent.Services
{
    public class DocumentProcessor : IDocumentProcessor
    {
        private readonly ILogger<DocumentProcessor> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _embeddingApiUrl = "http://embeddings-query-backend:9061/api/generate_embeddings"; // Updated for Docker Compose networking and new port

        public DocumentProcessor(ILogger<DocumentProcessor> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task ProcessAsync(SavedJobDescription jobDescription)
        {
            try
            {
                var payload = new
                {
                    job_id = jobDescription.Id.ToString(), // Ensure string type for API
                    description = jobDescription.Description
                };
                var response = await _httpClient.PostAsJsonAsync(_embeddingApiUrl, payload);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    
                    _logger.LogError($"DocumentProcessor embedding API failed: {response.StatusCode} - {error}");
                }
                else
                {
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Embedding API response: {apiResponse}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to process job description for embedding via query backend API.");
            }
        }
    }
}
