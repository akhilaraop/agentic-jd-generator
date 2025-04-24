namespace JobDescriptionAgent.Models
{
    public class AppSettings
    {
        public GroqApiSettings GroqApi { get; set; } = new();
    }

    public class GroqApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
} 