// Updated to load agent prompts from configuration (appsettings.json)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

    public class JDOrchestrator
    {
        private readonly ClarifierAgent _clarifier;
        private readonly GeneratorAgent _generator;
        private readonly CritiqueAgent _critic;
        private readonly ComplianceAgent _compliance;
        private readonly RewriterAgent _rewriter;

        public JDOrchestrator(LanguageModelService LanguageModelService, IConfiguration config)
        {
            _clarifier = new ClarifierAgent(LanguageModelService, config);
            _generator = new GeneratorAgent(LanguageModelService, config);
            _critic = new CritiqueAgent(LanguageModelService, config);
            _compliance = new ComplianceAgent(LanguageModelService, config);
            _rewriter = new RewriterAgent(LanguageModelService, config);
        }

        public async Task<JDResponse> RunAsync(string userInput)
        {
            var response = new JDResponse();
            
            var clarify = await _clarifier.ExecuteAsync(userInput);
            
            // Check if clarification is needed
            if (clarify.Trim().StartsWith("Need clarification"))
            {
                response.FinalJobDescription = $"Clarifier Agent needs more information:\n\n{clarify}";
                return response;
            }

            // Extract assumptions if any
            if (clarify.Contains("Making the following assumptions:"))
            {
                response.Assumptions = clarify.Split("Making the following assumptions:")[1].Trim();
            }

            // Generate all components
            response.InitialDraft = await _generator.ExecuteAsync(userInput);
            response.CritiqueFeedback = await _critic.ExecuteAsync(response.InitialDraft);
            response.ComplianceReview = await _compliance.ExecuteAsync(response.CritiqueFeedback);

            response.CombinedFeedback = $"Original JD:\n{response.InitialDraft}\n\n" +
                                      $"Critique Feedback:\n{response.CritiqueFeedback}\n\n" +
                                      $"Compliance Feedback:\n{response.ComplianceReview}";

            response.FinalJobDescription = await _rewriter.ExecuteAsync(response.CombinedFeedback);

            return response;
        }
    }
}
