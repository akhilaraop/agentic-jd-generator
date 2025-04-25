using System;

namespace JobDescriptionAgent.Models
{
    public class SavedJobDescription
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InitialInput { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Stages { get; set; } = new Dictionary<string, string>();
    }
} 