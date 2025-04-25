using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using JobDescriptionAgent.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace JobDescriptionAgent.Services
{
    public interface IAgentPromptService
    {
        AgentConfig GetAgentPrompt(string agentType);
        void ReloadPrompts();
    }

    public class AgentPromptService : IAgentPromptService
    {
        private readonly ILogger<AgentPromptService> _logger;
        private AgentPrompts _prompts;
        private readonly string _promptsPath;

        public AgentPromptService(ILogger<AgentPromptService> logger)
        {
            _logger = logger;
            
            // Initialize with empty values
            _prompts = new AgentPrompts
            {
                Clarifier = CreateEmptyAgentConfig(),
                Generator = CreateEmptyAgentConfig(),
                Critique = CreateEmptyAgentConfig(),
                Compliance = CreateEmptyAgentConfig(),
                Rewriter = CreateEmptyAgentConfig(),
                Finalizer = CreateEmptyAgentConfig()
            };
            
            // Try to find the YAML file in different locations
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "agent_prompts.yaml"),  // Project root
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent_prompts.yaml"),  // Output directory
                Path.Combine(Directory.GetCurrentDirectory(), "..", "agent_prompts.yaml")  // One level up
            };

            _promptsPath = possiblePaths.FirstOrDefault(File.Exists) 
                ?? throw new FileNotFoundException("Could not find agent_prompts.yaml in any of the expected locations.");

            _logger.LogInformation("Using prompts file from: {Path}", _promptsPath);
            LoadPrompts();
        }

        private AgentConfig CreateEmptyAgentConfig()
        {
            return new AgentConfig
            {
                Name = string.Empty,
                Description = string.Empty,
                Task = new List<string>(),
                Input = new object(),
                Output = new object(),
                OutputFormat = new OutputFormat
                {
                    Assumptions = new Dictionary<string, object>(),
                    NeedClarification = new Dictionary<string, object>(),
                    Critique = new List<string>(),
                    ComplianceIssues = new List<ComplianceIssue>(),
                    Text = string.Empty
                },
                Constraints = new List<string>(),
                StyleGuidelines = new StyleGuidelines { Guidelines = new List<string>() }
            };
        }

        private void LoadPrompts()
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .WithTypeConverter(new OutputFormatTypeConverter())
                    .WithTypeConverter(new StyleGuidelinesTypeConverter())
                    .Build();

                var yaml = File.ReadAllText(_promptsPath);
                _prompts = deserializer.Deserialize<AgentPrompts>(yaml);
                _logger.LogInformation("Successfully loaded agent prompts from {Path}", _promptsPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load agent prompts from {Path}", _promptsPath);
                throw new InvalidOperationException($"Failed to load agent prompts from {_promptsPath}", ex);
            }
        }

        public AgentConfig GetAgentPrompt(string agentType)
        {
            return agentType.ToLower() switch
            {
                "clarifier" => _prompts.Clarifier,
                "generator" => _prompts.Generator,
                "critique" => _prompts.Critique,
                "compliance" => _prompts.Compliance,
                "rewriter" => _prompts.Rewriter,
                "finalizer" => _prompts.Finalizer,
                _ => throw new ArgumentException($"Unknown agent type: {agentType}")
            };
        }

        public void ReloadPrompts()
        {
            LoadPrompts();
        }
    }

    public class OutputFormatTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(OutputFormat);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var value = string.Empty;
            
            if (parser.Current is Scalar scalar)
            {
                value = scalar.Value;
                parser.MoveNext();
            }
            else if (parser.Current is MappingStart)
            {
                var assumptions = new Dictionary<string, object>();
                var needClarification = new Dictionary<string, object>();
                var critique = new List<string>();
                var complianceIssues = new List<ComplianceIssue>();

                parser.MoveNext();
                while (parser.Current is not MappingEnd)
                {
                    if (parser.Current is Scalar key)
                    {
                        parser.MoveNext();
                        switch (key.Value)
                        {
                            case "assumptions":
                                if (parser.Current is MappingStart)
                                {
                                    parser.MoveNext();
                                    while (parser.Current is not MappingEnd)
                                    {
                                        if (parser.Current is Scalar assumptionKey)
                                        {
                                            parser.MoveNext();
                                            if (parser.Current is Scalar assumptionValue)
                                            {
                                                assumptions[assumptionKey.Value] = assumptionValue.Value;
                                            }
                                        }
                                        parser.MoveNext();
                                    }
                                }
                                break;
                            case "need_clarification":
                                if (parser.Current is MappingStart)
                                {
                                    parser.MoveNext();
                                    while (parser.Current is not MappingEnd)
                                    {
                                        if (parser.Current is Scalar clarificationKey)
                                        {
                                            parser.MoveNext();
                                            if (parser.Current is Scalar clarificationValue)
                                            {
                                                needClarification[clarificationKey.Value] = clarificationValue.Value;
                                            }
                                        }
                                        parser.MoveNext();
                                    }
                                }
                                break;
                            case "critique":
                                if (parser.Current is SequenceStart)
                                {
                                    parser.MoveNext();
                                    while (parser.Current is not SequenceEnd)
                                    {
                                        if (parser.Current is Scalar critiqueValue)
                                        {
                                            critique.Add(critiqueValue.Value);
                                        }
                                        parser.MoveNext();
                                    }
                                }
                                break;
                            case "compliance_issues":
                                if (parser.Current is SequenceStart)
                                {
                                    parser.MoveNext();
                                    while (parser.Current is not SequenceEnd)
                                    {
                                        if (parser.Current is MappingStart)
                                        {
                                            parser.MoveNext();
                                            var issue = new ComplianceIssue { Issue = string.Empty, Fix = string.Empty };
                                            while (parser.Current is not MappingEnd)
                                            {
                                                if (parser.Current is Scalar issueKey)
                                                {
                                                    parser.MoveNext();
                                                    if (parser.Current is Scalar issueValue)
                                                    {
                                                        switch (issueKey.Value)
                                                        {
                                                            case "issue":
                                                                issue.Issue = issueValue.Value;
                                                                break;
                                                            case "fix":
                                                                issue.Fix = issueValue.Value;
                                                                break;
                                                        }
                                                    }
                                                }
                                                parser.MoveNext();
                                            }
                                            complianceIssues.Add(issue);
                                        }
                                        parser.MoveNext();
                                    }
                                }
                                break;
                            case "text":
                                if (parser.Current is Scalar textValue)
                                {
                                    value = textValue.Value;
                                }
                                break;
                        }
                    }
                    parser.MoveNext();
                }
                parser.MoveNext();

                return new OutputFormat
                {
                    Assumptions = assumptions,
                    NeedClarification = needClarification,
                    Critique = critique,
                    ComplianceIssues = complianceIssues,
                    Text = value
                };
            }

            return new OutputFormat
            {
                Assumptions = new Dictionary<string, object>(),
                NeedClarification = new Dictionary<string, object>(),
                Critique = new List<string>(),
                ComplianceIssues = new List<ComplianceIssue>(),
                Text = value
            };
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is OutputFormat outputFormat)
            {
                if (!string.IsNullOrEmpty(outputFormat.Text))
                {
                    emitter.Emit(new Scalar(outputFormat.Text));
                }
                else
                {
                    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                    
                    if (outputFormat.Assumptions.Any())
                    {
                        emitter.Emit(new Scalar("assumptions"));
                        emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                        foreach (var assumption in outputFormat.Assumptions)
                        {
                            emitter.Emit(new Scalar(assumption.Key));
                            emitter.Emit(new Scalar(assumption.Value?.ToString() ?? string.Empty));
                        }
                        emitter.Emit(new MappingEnd());
                    }

                    if (outputFormat.NeedClarification.Any())
                    {
                        emitter.Emit(new Scalar("need_clarification"));
                        emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                        foreach (var clarification in outputFormat.NeedClarification)
                        {
                            emitter.Emit(new Scalar(clarification.Key));
                            emitter.Emit(new Scalar(clarification.Value?.ToString() ?? string.Empty));
                        }
                        emitter.Emit(new MappingEnd());
                    }

                    if (outputFormat.Critique.Any())
                    {
                        emitter.Emit(new Scalar("critique"));
                        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                        foreach (var critique in outputFormat.Critique)
                        {
                            emitter.Emit(new Scalar(critique));
                        }
                        emitter.Emit(new SequenceEnd());
                    }

                    if (outputFormat.ComplianceIssues.Any())
                    {
                        emitter.Emit(new Scalar("compliance_issues"));
                        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                        foreach (var issue in outputFormat.ComplianceIssues)
                        {
                            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                            emitter.Emit(new Scalar("issue"));
                            emitter.Emit(new Scalar(issue.Issue));
                            emitter.Emit(new Scalar("fix"));
                            emitter.Emit(new Scalar(issue.Fix));
                            emitter.Emit(new MappingEnd());
                        }
                        emitter.Emit(new SequenceEnd());
                    }

                    emitter.Emit(new MappingEnd());
                }
            }
        }
    }

    public class StyleGuidelinesTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(StyleGuidelines);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var guidelines = new List<string>();

            if (parser.Current is SequenceStart)
            {
                parser.MoveNext();
                while (parser.Current is not SequenceEnd)
                {
                    if (parser.Current is Scalar scalar)
                    {
                        guidelines.Add(scalar.Value);
                    }
                    parser.MoveNext();
                }
                parser.MoveNext();
            }
            else if (parser.Current is MappingStart)
            {
                parser.MoveNext();
                while (parser.Current is not MappingEnd)
                {
                    if (parser.Current is Scalar key && key.Value == "guidelines")
                    {
                        parser.MoveNext();
                        if (parser.Current is SequenceStart)
                        {
                            parser.MoveNext();
                            while (parser.Current is not SequenceEnd)
                            {
                                if (parser.Current is Scalar scalar)
                                {
                                    guidelines.Add(scalar.Value);
                                }
                                parser.MoveNext();
                            }
                            parser.MoveNext();
                        }
                    }
                    else
                    {
                        parser.MoveNext();
                    }
                    parser.MoveNext();
                }
                parser.MoveNext();
            }

            return new StyleGuidelines { Guidelines = guidelines };
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is StyleGuidelines styleGuidelines)
            {
                if (styleGuidelines.Guidelines.Any())
                {
                    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                    emitter.Emit(new Scalar("guidelines"));
                    emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                    foreach (var guideline in styleGuidelines.Guidelines)
                    {
                        emitter.Emit(new Scalar(guideline));
                    }
                    emitter.Emit(new SequenceEnd());
                    emitter.Emit(new MappingEnd());
                }
            }
        }
    }
} 