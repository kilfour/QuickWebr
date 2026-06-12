using HorsesForCourses.Domain.Courses.TimeSlots;

namespace HorsesForCourses.Api.Service.Courses.GetCourseDetail;

public record CourseDetail // Reused too much, check IsConfirmed
{
    public record TimeSlotInfo(CourseDay Day, int Start, int End);
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public IEnumerable<string> Skills { get; set; } = [];
    public IEnumerable<TimeSlotInfo> TimeSlots { get; set; } = [];
    public bool IsConfirmed { get; set; }
    public IdAndName? Coach { get; set; }
}