using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using JobDescriptionAgent.Models;

namespace JobDescriptionAgent.Services
{
    /// <summary>
    /// Service for interacting with the language model API (e.g., Groq/OpenAI).
    /// </summary>
    public class LanguageModelService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly Dictionary<string, (string? ApiKey, string BaseUrl, string Model)> _modelConfigs;

        public LanguageModelService(IOptions<AppSettings> options, IConfiguration config)
        {
            _httpClient = new HttpClient();
            _config = config;
            _modelConfigs = new Dictionary<string, (string?, string, string)>();

            // Load Groq config
            var groqSection = _config.GetSection("LanguageModels:Groq");
            _modelConfigs["llama3-8b-8192"] = (
                groqSection.GetValue<string>("ApiKey"),
                groqSection.GetValue<string>("BaseUrl"),
                groqSection.GetValue<string>("Model")
            );

            
        }

        /// <summary>
        /// Sends a prompt and user input to the language model and returns the response.
        /// </summary>
        /// <param name="prompt">The system prompt or instructions for the model.</param>
        /// <param name="userInput">The user's input or question.</param>
        /// <param name="modelKey">The model key to use ("llama3-8b-8192").</param>
        /// <returns>The model's response as a string.</returns>
        public async Task<string> AskAsync(string prompt, string userInput, string? modelKey = null)
        {
            var key = modelKey ?? _config["LanguageModels:Default"] ?? "llama3-8b-8192";
            if (!_modelConfigs.TryGetValue(key, out var config))
                throw new InvalidOperationException($"Unknown model key: {key}");

            var apiKey = config.ApiKey ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (key == "llama3-8b-8192" && string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("GROQ_API_KEY is missing in configuration.");

            var requestBody = new
            {
                model = config.Model,
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user", content = userInput }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, config.BaseUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            if (key == "llama3-8b-8192")
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}"
                );
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc
                .RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }
}
