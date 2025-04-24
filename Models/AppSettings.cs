namespace JobDescriptionAgent.Models
{
    public class AppSettings
    {
        public GroqApiSettings GroqApi { get; set; } = new();
        public Prompts Prompts { get; set; } = new();
    }

    public class GroqApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
        public string Model { get; set; } = "mixtral-8x7b-32768";
        public double Temperature { get; set; } = 0.7;
    }

    public class Prompts
    {
        public string ClarifierPrompt { get; set; } = string.Empty;
        public string GeneratorPrompt { get; set; } = string.Empty;
        public string CritiquePrompt { get; set; } = string.Empty;
        public string CompliancePrompt { get; set; } = string.Empty;
        public string RewriterPrompt { get; set; } = string.Empty;
    }
} 