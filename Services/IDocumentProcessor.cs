using System.Threading.Tasks;
using JobDescriptionAgent.Models;

namespace JobDescriptionAgent.Services
{
    public interface IDocumentProcessor
    {
        Task ProcessAsync(SavedJobDescription jobDescription);
    }
}
