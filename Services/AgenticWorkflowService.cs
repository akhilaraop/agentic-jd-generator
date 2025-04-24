// Updated to load agent prompts from configuration (appsettings.json)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using JobDescriptionAgent.Models;

namespace JobDescriptionAgent.Services
{
    public interface IAgent
    {
        string Name { get; }
        string Role { get; }
        string Goal { get; }
        Task<string> ExecuteAsync(string input);
    }

    public abstract class AgentBase : IAgent
    {
        protected readonly LanguageModelService _LanguageModelService;
        protected readonly string _model;
        protected readonly string _prompt;
        protected AgentBase(LanguageModelService LanguageModelService, IConfiguration config, string promptKey)
        {
            _LanguageModelService = LanguageModelService;
            _model = config["LLM:Model"] ?? "llama3-8b-8192";
            _prompt = config[$"Prompts:{promptKey}"] ?? string.Empty;
        }

        public abstract string Name { get; }
        public abstract string Role { get; }
        public abstract string Goal { get; }
        public abstract Task<string> ExecuteAsync(string input);
    }

    public class ClarifierAgent : AgentBase
    {
        public ClarifierAgent(LanguageModelService LanguageModelService, IConfiguration config) : base(LanguageModelService, config, "Clarifier") { }
        public override string Name => "ClarifierAgent";
        public override string Role => "Clarifier";
        public override string Goal => "Clarify vague inputs and make reasonable assumptions";
        public override async Task<string> ExecuteAsync(string input) => await _LanguageModelService.AskAsync(_prompt, input, _model);
    }

    public class GeneratorAgent : AgentBase
    {
        public GeneratorAgent(LanguageModelService LanguageModelService, IConfiguration config) : base(LanguageModelService, config, "Generator") { }
        public override string Name => "GeneratorAgent";
        public override string Role => "Generator";
        public override string Goal => "Generate the initial job description";
        public override async Task<string> ExecuteAsync(string input) => await _LanguageModelService.AskAsync(_prompt, input, _model);
    }

    public class CritiqueAgent : AgentBase
    {
        public CritiqueAgent(LanguageModelService LanguageModelService, IConfiguration config) : base(LanguageModelService, config, "Critique") { }
        public override string Name => "CritiqueAgent";
        public override string Role => "Critic";
        public override string Goal => "Polish the tone, formatting, and inclusiveness";
        public override async Task<string> ExecuteAsync(string input) => await _LanguageModelService.AskAsync(_prompt, input, _model);
    }

    public class ComplianceAgent : AgentBase
    {
        public ComplianceAgent(LanguageModelService LanguageModelService, IConfiguration config) : base(LanguageModelService, config, "Compliance") { }
        public override string Name => "ComplianceAgent";
        public override string Role => "Compliance Reviewer";
        public override string Goal => "Check for bias, EEOC compliance, vague requirements";
        public override async Task<string> ExecuteAsync(string input) => await _LanguageModelService.AskAsync(_prompt, input, _model);
    }

    public class RewriterAgent : AgentBase
    {
        public RewriterAgent(LanguageModelService LanguageModelService, IConfiguration config) : base(LanguageModelService, config, "Rewriter") { }
        public override string Name => "RewriterAgent";
        public override string Role => "Improver";
        public override string Goal => "Rewrite the JD using feedback from critique and compliance";
        public override async Task<string> ExecuteAsync(string input) => await _LanguageModelService.AskAsync(_prompt, input, _model);
    }

    public class JDOrchestrator : IAgenticWorkflowService
    {
        private readonly LanguageModelService _languageModelService;
        private readonly IOptions<AppSettings> _settings;
        private readonly ILogger<JDOrchestrator> _logger;

        public JDOrchestrator(
            LanguageModelService languageModelService,
            IOptions<AppSettings> settings,
            ILogger<JDOrchestrator> logger)
        {
            _languageModelService = languageModelService;
            _settings = settings;
            _logger = logger;
        }

        public async Task<(string description, Dictionary<string, string> stages)> RunAsync(string input)
        {
            try
            {
                var stages = new Dictionary<string, string>();

                // Step 1: Clarification
                var clarificationResponse = await _languageModelService.AskAsync(
                    _settings.Value.Prompts.ClarifierPrompt,
                    input
                );
                stages["clarity"] = clarificationResponse;

                if (clarificationResponse.Contains("Need clarification", StringComparison.OrdinalIgnoreCase))
                {
                    return (string.Empty, stages);
                }

                string assumptions = string.Empty;
                if (clarificationResponse.Contains("Making the following assumptions:", StringComparison.OrdinalIgnoreCase))
                {
                    assumptions = clarificationResponse;
                }

                // Step 2: Generation
                var generatedJD = await _languageModelService.AskAsync(
                    _settings.Value.Prompts.GeneratorPrompt,
                    input
                );
                stages["initial"] = generatedJD;

                // Step 3: Critique
                var critique = await _languageModelService.AskAsync(
                    _settings.Value.Prompts.CritiquePrompt,
                    generatedJD
                );
                stages["critique"] = critique;

                // Step 4: Compliance Check
                var complianceCheck = await _languageModelService.AskAsync(
                    _settings.Value.Prompts.CompliancePrompt,
                    generatedJD
                );
                stages["compliance"] = complianceCheck;

                // Step 5: Final Rewrite
                var finalJD = await _languageModelService.AskAsync(
                    _settings.Value.Prompts.RewriterPrompt,
                    $"Original JD:\n{generatedJD}\n\nCritique:\n{critique}\n\nCompliance Notes:\n{complianceCheck}"
                );

                var notes = string.Join("\n\n", new[] { assumptions, critique, complianceCheck }
                    .Where(n => !string.IsNullOrEmpty(n)));

                return (finalJD, stages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JD generation workflow");
                throw;
            }
        }
    }
}
