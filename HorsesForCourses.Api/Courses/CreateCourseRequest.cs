namespace HorsesForCourses.Api.Courses;

public record CreateCourseRequest(string Name, DateOnly StartDate, DateOnly EndDate);
