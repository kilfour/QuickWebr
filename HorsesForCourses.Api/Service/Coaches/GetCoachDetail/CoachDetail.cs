namespace HorsesForCourses.Api.Service.Coaches.GetCoachDetail;

public record CoachDetail
{
    public record CourseInfo(int Id, string Name);
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public List<CourseInfo> Courses { get; set; } = [];
}
