using System.Text;
using System.Text.Json;
using JobDescriptionAgent.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace JobDescriptionAgent.Services
{
    /// <summary>
    /// Service for interacting with the language model API (e.g., Groq/OpenAI).
    /// </summary>
    public class LanguageModelService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly Dictionary<
            string,
            (string? ApiKey, string BaseUrl, string Model)
        > _modelConfigs;

        public LanguageModelService(IOptions<AppSettings> options, IConfiguration config)
        {
            _config = config;
            _modelConfigs = new Dictionary<string, (string?, string, string)>();

            // Load Groq config
            var groqSection = _config.GetSection("GroqApi");
            var baseUrl = groqSection.GetValue<string>("BaseUrl");
            var model = groqSection.GetValue<string>("Model");
            
            _modelConfigs["llama3-8b-8192"] = (
                Environment.GetEnvironmentVariable("GROQ_API_KEY"),
                baseUrl,
                model
            );

            // Configure HttpClient
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
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
            var key = modelKey ?? "llama3-8b-8192";
            if (!_modelConfigs.TryGetValue(key, out var config))
                throw new InvalidOperationException($"Unknown model key: {key}");

            var apiKey = config.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("GROQ_API_KEY is missing in configuration.");

            var requestBody = new
            {
                model = config.Model,
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user", content = userInput },
                },
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                ),
            };
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
