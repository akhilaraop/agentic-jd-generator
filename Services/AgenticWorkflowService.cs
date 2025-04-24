
using JobDescriptionAgent.Models;
using System.Threading.Tasks;

namespace JobDescriptionAgent.Services
{
    public class AgenticWorkflowService
    {
        private readonly LlamaService _llama;

        public AgenticWorkflowService(LlamaService llamaService)
        {
            _llama = llamaService;
        }

        public async Task<(string JobDescription, string ComplianceReview)> RunAgenticWorkflowAsync(string initialInput)
        {
            var clarifierPrompt = @"
            You are a Clarifier Agent helping to generate job descriptions. 
            The user may provide vague or incomplete information. 
            Your job is to ask clarifying questions before any description is written.

            Do not attempt to write a job description.  
            Do not assume missing information.  
            Only respond with a numbered list of specific questions you would ask the user.

            If everything needed for a job description is provided (role, level, location, skills, responsibilities, etc.), respond with: 'No clarifications needed.'";

            var clarify = await _llama.AskAsync(clarifierPrompt, initialInput);

            // if (!clarify.Trim().ToLower().StartsWith("no clarifications needed"))
            // {
            //     return ($"Clarifier Agent needs more information:\n\n{clarify}", "");
            // }

            var generatePrompt = @"
            You are a JD Generator Agent. Use the user's input to write a professional job description.
            Structure it with: About Us, Role, Responsibilities, Requirements, Perks.";

            var generate = await _llama.AskAsync(generatePrompt, initialInput);

            var critiquePrompt = @"
            You are a Critique Agent. Review and revise the job description for tone, clarity, and formatting.
            Ensure it's inclusive, modern, and professionally written.";

            var critique = await _llama.AskAsync(critiquePrompt, generate);

            var compliancePrompt = @"
            You are a Compliance & Fairness Agent for job descriptions. 
            Your responsibilities include:
            - Identifying biased or exclusionary language
            - Ensuring the JD is inclusive and aligned with EEOC best practices
            - Suggesting improvements to skill requirements or vague role definitions
            - Highlighting any missing or under-explained responsibilities

            Respond with a bullet-point summary of any issues found and how to improve them.";

            var complianceReview = await _llama.AskAsync(compliancePrompt, critique);

            return (critique, complianceReview);
        }


    }
}
