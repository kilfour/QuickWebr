using HorsesForCourses.Abstractions;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HorsesForCourses.Api.Service.Courses.GetCourseDetail;

public interface IGetCourseDetail
{
    Task<CourseDetail?> One(int id);
}

public class GetCourseDetail(AppDbContext dbContext) : IGetCourseDetail
{
    private readonly AppDbContext dbContext = dbContext;

    public async Task<CourseDetail?> One(int id)
    {
        var courseId = Id<Course>.From(id);
        return await dbContext.Courses
            .AsNoTracking()
            .Where(a => a.Id == courseId)
            .Select(a => new CourseDetail
            {
                Id = a.Id.Value,
                Name = a.Name.Value,
                Start = a.Period.Start,
                End = a.Period.End,
                Skills = a.RequiredSkills.Select(b => b.Value),
                TimeSlots = a.TimeSlots.Select(b => new CourseDetail.TimeSlotInfo(b.Day, b.Start.Value, b.End.Value)),
                IsConfirmed = a.IsConfirmed,
                Coach = a.AssignedCoach == null ? null : new IdAndName(a.AssignedCoach.Id.Value, a.AssignedCoach.Name.Value)

            }).SingleOrDefaultAsync();
    }
}