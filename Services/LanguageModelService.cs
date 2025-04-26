using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using JobDescriptionAgent.Models;


namespace JobDescriptionAgent.Services
{
    /// <summary>
    /// Service for interacting with the language model API (e.g., Groq/OpenAI).
    /// </summary>
    public class LanguageModelService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqApiSettings _apiSettings;

        public LanguageModelService(IOptions<AppSettings> options)
        {
            _httpClient = new HttpClient();
            _apiSettings = options.Value.GroqApi ?? throw new InvalidOperationException("GroqApi settings are missing in configuration.");
            
            if (string.IsNullOrEmpty(_apiSettings.ApiKey))
                throw new InvalidOperationException("GROQ_API_KEY is missing in configuration.");
        }

        /// <summary>
        /// Sends a prompt and user input to the language model and returns the response.
        /// </summary>
        /// <param name="prompt">The system prompt or instructions for the model.</param>
        /// <param name="userInput">The user's input or question.</param>
        /// <param name="modelOverride">Optional model override (defaults to configured model).</param>
        /// <returns>The model's response as a string.</returns>
        public async Task<string> AskAsync(string prompt, string userInput, string? modelOverride = null)
        {
            var fullPrompt = $"{prompt}\nUser input: {userInput}";

            var requestBody = new
            {
                model = modelOverride ?? _apiSettings.Model,
                messages = new[] { new { role = "user", content = fullPrompt } },
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _apiSettings.BaseUrl)
            {
                Headers = { { "Authorization", $"Bearer {_apiSettings.ApiKey}" } },
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

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
