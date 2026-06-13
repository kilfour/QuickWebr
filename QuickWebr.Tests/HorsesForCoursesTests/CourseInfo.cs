namespace QuickWebr.Tests.HorsesForCoursesTests;

public record CourseInfo(int Id)
{
    public List<string> RequiredSkills { get; init; } = [];
    public bool HasTimeSlots { get; init; } = false;
    public bool IsConfirmed { get; init; } = false;
}
