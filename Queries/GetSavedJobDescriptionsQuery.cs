using MediatR;
using JobDescriptionAgent.Models;
using JobDescriptionAgent.Data;
using Microsoft.EntityFrameworkCore;

namespace JobDescriptionAgent.Queries
{
    public class GetSavedJobDescriptionsQuery : IRequest<List<SavedJobDescription>> { }

    public class GetSavedJobDescriptionsQueryHandler : IRequestHandler<GetSavedJobDescriptionsQuery, List<SavedJobDescription>>
    {
        private readonly ApplicationDbContext _context;
        public GetSavedJobDescriptionsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<SavedJobDescription>> Handle(GetSavedJobDescriptionsQuery request, CancellationToken cancellationToken)
        {
            return await _context.SavedJobDescriptions
                .OrderByDescending(jd => jd.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
} 