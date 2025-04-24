using System.Threading.Tasks;

namespace JobDescriptionAgent.Services
{
    public interface IAgenticWorkflowService
    {
        Task<(string description, string notes)> RunAsync(string input);
    }
} 