
namespace JobDescriptionAgent.Services
{
    public class AgenticWorkflowService
    {
        private readonly LanguageModelService _languagemodel;

        public AgenticWorkflowService(LanguageModelService LanguageModelService)
        {
            _languagemodel = LanguageModelService;
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

            var clarify = await _languagemodel.AskAsync(clarifierPrompt, initialInput);

            /* if (!clarify.Trim().ToLower().StartsWith("no clarifications needed"))
            {
                return ($"Clarifier Agent needs more information:\n\n{clarify}", "");
            } */

            // STEP 1: Initial Generation
            var generatePrompt = @"
            You are a JD Generator Agent. Use the user's input to write a professional job description.
            Structure it with: About Us, Role, Responsibilities, Requirements, Perks.";

            var generate = await _languagemodel.AskAsync(generatePrompt, initialInput);

            // STEP 2: Critique
            var critiquePrompt = @"
            You are a Critique Agent. Review and revise the job description for tone, clarity, and formatting.
            Ensure it's inclusive, modern, and professionally written.";

            var critique = await _languagemodel.AskAsync(critiquePrompt, generate);

            // ⚖️ STEP 3: Compliance Check
            var compliancePrompt = @"
            You are a Compliance & Fairness Agent for job descriptions. 
            Your responsibilities include:
            - Identifying biased or exclusionary language
            - Ensuring the JD is inclusive and aligned with EEOC best practices
            - Suggesting improvements to skill requirements or vague role definitions
            - Highlighting any missing or under-explained responsibilities

            Respond with a bullet-point summary of any issues found and how to improve them.";

            var complianceReview = await _languagemodel.AskAsync(compliancePrompt, critique);


            // 🔁 STEP 4: Rewrite with Feedback
            var rewriterPrompt = @"
            You are a JD Rewriter Agent. 
            You are given a job description along with critique and compliance feedback.
            Your task is to rewrite the job description incorporating all suggested improvements.

            Maintain a clear structure:
            - About Us
            - Role
            - Responsibilities
            - Requirements
            - Perks

            Only return the improved job description.";

            var rewriterInput = $@"
            Original JD:
            {generate}

            Critique Feedback:
            {critique}

            Compliance Feedback:
            {complianceReview}
            ";

            var improvedJD = await _languagemodel.AskAsync(rewriterPrompt, rewriterInput);

            return (improvedJD, complianceReview);
        }
    }
}
