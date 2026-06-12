using HorsesForCourses.Abstractions;

namespace HorsesForCourses.Api.Service.Warehouse;

public interface IAmASuperVisor
{
    Task Enlist(IDomainEntity entity);
    Task Ship();
}

public class DataSupervisor(AppDbContext dbContext) : IAmASuperVisor
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task Enlist(IDomainEntity entity)
    {
        await dbContext.AddAsync(entity);
    }

    public async Task Ship()
    {
        await dbContext.SaveChangesAsync();
    }
}
