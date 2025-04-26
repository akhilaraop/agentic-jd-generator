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
        protected readonly IAgentPromptService _promptService;
        protected readonly string _model;
        protected readonly string _agentType;

        protected AgentBase(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config,
            string agentType)
        {
            _LanguageModelService = LanguageModelService;
            _promptService = promptService;
            _model = config["LLM:Model"] ?? "llama3-8b-8192";
            _agentType = agentType;
        }

        public abstract string Name { get; }
        public abstract string Role { get; }
        public abstract string Goal { get; }

        public virtual async Task<string> ExecuteAsync(string input)
        {
            var agentConfig = _promptService.GetAgentPrompt(_agentType);
            var prompt = $"{agentConfig.Description}\n\nTask:\n{string.Join("\n", agentConfig.Task)}\n\nConstraints:\n{string.Join("\n", agentConfig.Constraints ?? new List<string>())}";
            return await _LanguageModelService.AskAsync(prompt, input, _model);
        }
    }

    public class ClarifierAgent : AgentBase
    {
        public ClarifierAgent(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config)
            : base(LanguageModelService, promptService, config, "clarifier") { }

        public override string Name => "ClarifierAgent";
        public override string Role => "Clarifier";
        public override string Goal => "Clarify vague inputs and make reasonable assumptions";
    }

    public class GeneratorAgent : AgentBase
    {
        public GeneratorAgent(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config)
            : base(LanguageModelService, promptService, config, "generator") { }

        public override string Name => "GeneratorAgent";
        public override string Role => "Generator";
        public override string Goal => "Generate the initial job description";
    }

    public class CritiqueAgent : AgentBase
    {
        public CritiqueAgent(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config)
            : base(LanguageModelService, promptService, config, "critique") { }

        public override string Name => "CritiqueAgent";
        public override string Role => "Critic";
        public override string Goal => "Polish the tone, formatting, and inclusiveness";
    }

    public class ComplianceAgent : AgentBase
    {
        public ComplianceAgent(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config)
            : base(LanguageModelService, promptService, config, "compliance") { }

        public override string Name => "ComplianceAgent";
        public override string Role => "Compliance Reviewer";
        public override string Goal => "Check for bias, EEOC compliance, vague requirements";
    }

    public class RewriterAgent : AgentBase
    {
        public RewriterAgent(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config)
            : base(LanguageModelService, promptService, config, "rewriter") { }

        public override string Name => "RewriterAgent";
        public override string Role => "Improver";
        public override string Goal => "Rewrite the JD using feedback from critique and compliance";
    }

    public class FinalizerAgent : AgentBase
    {
        public FinalizerAgent(
            LanguageModelService LanguageModelService,
            IAgentPromptService promptService,
            IConfiguration config)
            : base(LanguageModelService, promptService, config, "finalizer") { }

        public override string Name => "FinalizerAgent";
        public override string Role => "Finalizer";
        public override string Goal => "Polish and enhance the final job description for maximum impact";

        public override async Task<string> ExecuteAsync(string input)
        {
            var agentConfig = _promptService.GetAgentPrompt(_agentType);
            var prompt = $"{agentConfig.Description}\n\nTask:\n{string.Join("\n", agentConfig.Task)}\n\nStyle Guidelines:\n{string.Join("\n", agentConfig.StyleGuidelines?.Guidelines ?? new List<string>())}\n\nConstraints:\n{string.Join("\n", agentConfig.Constraints ?? new List<string>())}";
            return await _LanguageModelService.AskAsync(prompt, input, _model);
        }
    }

    /// <summary>
    /// Orchestrates the multi-agent workflow for generating job descriptions.
    /// </summary>
    public class JDOrchestrator : IAgenticWorkflowService
    {
        private readonly LanguageModelService _languageModelService;
        private readonly IAgentPromptService _promptService;
        private readonly IConfiguration _config;
        private readonly ILogger<JDOrchestrator> _logger;

        private readonly ClarifierAgent _clarifier;
        private readonly GeneratorAgent _generator;
        private readonly CritiqueAgent _critique;
        private readonly ComplianceAgent _compliance;
        private readonly RewriterAgent _rewriter;
        private readonly FinalizerAgent _finalizer;

        public JDOrchestrator(
            LanguageModelService languageModelService,
            IAgentPromptService promptService,
            IConfiguration config,
            ILogger<JDOrchestrator> logger)
        {
            _languageModelService = languageModelService;
            _promptService = promptService;
            _config = config;
            _logger = logger;

            // Initialize agents
            _clarifier = new ClarifierAgent(languageModelService, promptService, config);
            _generator = new GeneratorAgent(languageModelService, promptService, config);
            _critique = new CritiqueAgent(languageModelService, promptService, config);
            _compliance = new ComplianceAgent(languageModelService, promptService, config);
            _rewriter = new RewriterAgent(languageModelService, promptService, config);
            _finalizer = new FinalizerAgent(languageModelService, promptService, config);
        }

        /// <summary>
        /// Runs the full job description generation workflow.
        /// </summary>
        /// <param name="input">The initial job description requirements provided by the user.</param>
        /// <returns>A tuple containing the final job description and a dictionary of all workflow stages.</returns>
        public async Task<(string description, Dictionary<string, string> stages)> RunAsync(string input)
        {
            try
            {
                var stages = new Dictionary<string, string>();

                // Step 1: Clarification
                var clarificationResponse = await _clarifier.ExecuteAsync(input);
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
                var generatedJD = await _generator.ExecuteAsync(input);
                stages["initial"] = generatedJD;

                // Step 3: Critique
                var critique = await _critique.ExecuteAsync(generatedJD);
                stages["critique"] = critique;

                // Step 4: Compliance Check
                var complianceCheck = await _compliance.ExecuteAsync(generatedJD);
                stages["compliance"] = complianceCheck;

                // Step 5: Rewrite with feedback
                var rewrittenJD = await _rewriter.ExecuteAsync(
                    $"Original JD:\n{generatedJD}\n\nCritique:\n{critique}\n\nCompliance Notes:\n{complianceCheck}"
                );
                stages["rewrite"] = rewrittenJD;

                // Step 6: Final polish
                var finalJD = await _finalizer.ExecuteAsync(rewrittenJD);
                stages["final"] = finalJD;

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
