using MediatR;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Data;
using Microsoft.EntityFrameworkCore;

namespace JobDescriptionAgent.Queries
{
    /// <summary>
    /// Query to retrieve all saved job descriptions.
    /// </summary>
    /// <remarks>
    /// Output: List of SavedJobDescription objects.
    /// </remarks>
    public class GetSavedJobDescriptionsQuery : IRequest<List<SavedJobDescription>> { }

    /// <summary>
    /// Handler for <see cref="GetSavedJobDescriptionsQuery"/>. Retrieves all saved job descriptions from the database.
    /// </summary>
    public class GetSavedJobDescriptionsQueryHandler : IRequestHandler<GetSavedJobDescriptionsQuery, List<SavedJobDescription>>
    {
        private readonly ApplicationDbContext _context;
        public GetSavedJobDescriptionsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Handles the query to retrieve all saved job descriptions.
        /// </summary>
        /// <param name="request">The query request (no parameters).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of <see cref="SavedJobDescription"/> objects.</returns>
        public async Task<List<SavedJobDescription>> Handle(GetSavedJobDescriptionsQuery request, CancellationToken cancellationToken)
        {
            return await _context.SavedJobDescriptions
                .OrderByDescending(jd => jd.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
} 