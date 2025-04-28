using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JobDescriptionAgent.Services
{
    public interface IQueryProcessor
    {
        Task<string> QueryAsync(string jobId, string query);
    }

    public class QueryProcessor : IQueryProcessor
    {
        private readonly ILogger<QueryProcessor> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _queryApiUrl = "http://query-backend:9061/api/query/id";

        public QueryProcessor(ILogger<QueryProcessor> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> QueryAsync(string jobId, string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = "0";
                }
                var payload = new { job_id = jobId, query = query };
                var response = await _httpClient.PostAsJsonAsync(_queryApiUrl, payload);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"QueryProcessor API failed: {response.StatusCode} - {content}");
                    
                    throw new Exception($"Query API error: {content}");
                }
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process chat query via query backend API.");
                throw;
            }
        }
    }
}
