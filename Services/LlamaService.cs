using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace JobDescriptionAgent.Services
{
    public class LlamaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public LlamaService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["GROQ_API_KEY"] ?? throw new InvalidOperationException("GROQ_API_KEY is missing in configuration.");
        }

        public async Task<string> AskAsync(string prompt, string userInput)
        {
            var fullPrompt = $"{prompt}\nUser input: {userInput}";

            var requestBody = new
            {
                model = "llama3-8b-8192",
                messages = new[]
            {
            new { role = "user", content = fullPrompt }
            }
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);



            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;








        }
    }
}
