using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Api.Service.Warehouse.Paging;
using Microsoft.EntityFrameworkCore;

namespace HorsesForCourses.Api.Service.Coaches.GetCoaches;

public interface IGetCoachSummaries
{
    Task<PagedResult<CoachSummary>> Paged(PageRequest request);
}

public class GetCoachSummaries(AppDbContext dbContext) : IGetCoachSummaries
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task<PagedResult<CoachSummary>> Paged(PageRequest request)
    {
        return await dbContext.Coaches
            .AsNoTracking()
            .OrderBy(p => p.Name.Value).ThenBy(p => p.Id)
            .Select(p => new CoachSummary(
                p.Id.Value,
                p.Name.Value,
                p.Email.Value,
                p.AssignedCourses.Count))
            .ToPagedResultAsync(request);
    }
}