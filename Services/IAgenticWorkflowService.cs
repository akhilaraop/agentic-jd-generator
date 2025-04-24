using System.Threading.Tasks;
using System.Collections.Generic;

namespace JobDescriptionAgent.Services
{
    public interface IAgenticWorkflowService
    {
        Task<(string description, Dictionary<string, string> stages)> RunAsync(string input);
    }
} 