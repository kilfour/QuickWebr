using HorsesForCourses.Api.Service.Coaches.GetCoachById;
using HorsesForCourses.Api.Service.Coaches.GetCoachDetail;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Domain.Accounts;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Api.Service.Coaches.GetCoaches;
using HorsesForCourses.Api.Service.Warehouse.Paging;

namespace HorsesForCourses.Api.Service.Coaches;

public interface ICoachesService
{
    Task<int> RegisterCoach(Actor actor, string name, string email);
    Task<bool> UpdateSkills(Actor actor, int id, IEnumerable<string> skills);
    Task<PagedResult<CoachSummary>> GetCoaches(int page, int pageSize);
    Task<CoachDetail?> GetCoachDetail(Actor actor, int id);
}

public class CoachesService(
    IAmASuperVisor Supervisor,
    IGetCoachById GetCoachById,
    IGetCoachSummaries GetCoachSummaries,
    IGetCoachDetailQuery GetCoachDetailQuery) : ICoachesService
{
    public async Task<int> RegisterCoach(Actor actor, string name, string email)
    {
        var coach = Coach.Create(actor, name, email);
        await Supervisor.Enlist(coach);
        await Supervisor.Ship();
        return coach.Id.Value;
    }

    public async Task<bool> UpdateSkills(Actor actor, int id, IEnumerable<string> skills)
    {
        var coach = await GetCoachById.One(id);
        if (coach == null) return false;
        coach.UpdateSkills(actor, skills);
        await Supervisor.Ship();
        return true;
    }

    public async Task<PagedResult<CoachSummary>> GetCoaches(int page, int pageSize)
        => await GetCoachSummaries.Paged(new PageRequest(page, pageSize));

    public async Task<CoachDetail?> GetCoachDetail(Actor actor, int id)
        => await GetCoachDetailQuery.One(actor, id);
}