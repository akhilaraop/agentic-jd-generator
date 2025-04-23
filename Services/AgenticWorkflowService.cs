
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

        public async Task<string> RunAgenticWorkflowAsync(string initialInput)
        {
            var clarify = await _llama.AskAsync(
                "You are a Clarifier Agent helping to generate job descriptions. The user may provide vague or incomplete information. Your job is to ask clarifying questions before any description is written. Do not attempt to write a job description. Do not assume missing information. Only respond with a numbered list of specific questions you would ask the user. If everything needed for a job description is provided (role, level, location, skills, responsibilities, etc.), respond with: 'No clarifications needed.'",
                initialInput);



            var generate = await _llama.AskAsync(
                "You are a JD Generator Agent. Use the inputs to write a job description for the specified role. Structure it with: About Us, Role, Responsibilities, Requirements, Perks.",
                clarify);

            var critique = await _llama.AskAsync(
                "You are a Critique Agent. Review and revise the job description for tone, clarity, and formatting. Ensure it's inclusive and polished.",
                generate);

            return critique;
        }

    }
}
