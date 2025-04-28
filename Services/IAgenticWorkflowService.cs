using System.Threading.Tasks;
using System.Collections.Generic;

namespace JobDescriptionAgent.Services
{
    /// <summary>
    /// Interface for the agentic workflow service that generates job descriptions.
    /// </summary>
    public interface IAgenticWorkflowService
    {
        /// <summary>
        /// Runs the job description generation workflow.
        /// </summary>
        /// <param name="input">The initial job description requirements provided by the user.</param>
        /// <param name="modelKey">The model key to use for the workflow.</param>
        /// <returns>A tuple containing the final job description and a dictionary of all workflow stages.</returns>
        Task<(string description, Dictionary<string, string> stages)> RunAsync(string input, string modelKey);
    }
} 