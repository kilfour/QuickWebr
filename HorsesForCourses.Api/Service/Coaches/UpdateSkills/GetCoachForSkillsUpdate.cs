using HorsesForCourses.Abstractions;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Api.Service.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace HorsesForCourses.Api.Service.Coaches.UpdateSkills;

public interface IGetCoachForSkillsUpdate
{
    Task<Coach?> One(int id);
}

public class GetCoachForSkillsUpdate(AppDbContext dbContext) : IGetCoachForSkillsUpdate
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task<Coach?> One(int id) =>
        await dbContext.Set<Coach>()
            .Include(c => c.Skills)
            .SingleOrDefaultAsync(c => c.Id == Id<Coach>.From(id));
}