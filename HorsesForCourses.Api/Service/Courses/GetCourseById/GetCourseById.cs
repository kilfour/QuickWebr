using HorsesForCourses.Abstractions;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Domain.Courses;

namespace HorsesForCourses.Api.Service.Courses.GetCourseById;

public interface IGetCourseById
{
    Task<Course?> Load(int id);
}

public class GetCourseById(AppDbContext dbContext) : IGetCourseById
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task<Course?> Load(int id) =>
        await dbContext.FindAsync<Course>(Id<Course>.From(id));
}