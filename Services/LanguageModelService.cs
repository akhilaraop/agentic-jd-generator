using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using JobDescriptionAgent.Models;


namespace JobDescriptionAgent.Services
{
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

        public async Task<string> AskAsync(string prompt, string userInput)
        {
            var fullPrompt = $"{prompt}\nUser input: {userInput}";

            var requestBody = new
            {
                model = _apiSettings.Model,
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
