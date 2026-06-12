namespace HorsesForCourses.Api.Service.Courses.GetCourses;

public record CourseSummary(
    int Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool HasSchedule,
    bool HasCoach);
