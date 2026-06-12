using HorsesForCourses.Abstractions;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Api.Service.Warehouse;

namespace HorsesForCourses.Api.Service.Coaches.GetCoachById;

public interface IGetCoachById
{
    Task<Coach?> One(int id);
}

public class GetCoachById(AppDbContext dbContext) : IGetCoachById
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task<Coach?> One(int id)
    {
        return await dbContext.FindAsync<Coach>(Id<Coach>.From(id));
    }
}