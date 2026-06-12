namespace QuickWebr.Tests.HorsesForCoursesTests;

public record CoachInfo(int Id)
{
    public List<string> Skills { get; init; } = [];
};
