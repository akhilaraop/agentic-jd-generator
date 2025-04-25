using System.Collections.Generic;

namespace JobDescriptionAgent.Models
{
    public class AgentPrompt
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<string> Task { get; set; }
        public required object Input { get; set; }
        public required object Output { get; set; }
        public required List<string> Constraints { get; set; }
        public required string OutputFormat { get; set; }
        public required StyleGuidelines StyleGuidelines { get; set; }
    }

    public class StyleGuidelines
    {
        public required List<string> Guidelines { get; set; }
    }

    public class AgentPrompts
    {
        public required AgentConfig Clarifier { get; set; }
        public required AgentConfig Generator { get; set; }
        public required AgentConfig Critique { get; set; }
        public required AgentConfig Compliance { get; set; }
        public required AgentConfig Rewriter { get; set; }
        public required AgentConfig Finalizer { get; set; }
    }

    public class AgentConfig
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<string> Task { get; set; }
        public required object Input { get; set; }
        public required object Output { get; set; }
        public required OutputFormat OutputFormat { get; set; }
        public required List<string> Constraints { get; set; }
        public required StyleGuidelines StyleGuidelines { get; set; }
    }

    public class OutputFormat
    {
        public required Dictionary<string, object> Assumptions { get; set; }
        public required Dictionary<string, object> NeedClarification { get; set; }
        public required List<string> Critique { get; set; }
        public required List<ComplianceIssue> ComplianceIssues { get; set; }
        public required string Text { get; set; }
    }

    public class ComplianceIssue
    {
        public required string Issue { get; set; }
        public required string Fix { get; set; }
    }
} 