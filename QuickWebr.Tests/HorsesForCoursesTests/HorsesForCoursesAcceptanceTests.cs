using HorsesForCourses.Abstractions;
using HorsesForCourses.Api.Coaches;
using HorsesForCourses.Api.Courses;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.Domain.Courses;
using HorsesForCourses.Domain.Courses.TimeSlots;
using HorsesForCourses.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickWebr.Tests.Tools;
using WibblyWobbly;

namespace QuickWebr.Tests.HorsesForCoursesTests;

[DocFile]
[DocFileHeader("ContactManager Acceptance Tests")]
[DocBoldHeader("WebApplicationFactory")]
[DocExample(typeof(WebrApplicationFactory))]
[DocWebrHeader]
[DocWebr]
[DocMethodsHeader]
[DocExample(typeof(CreateCoach))]
[DocExample(typeof(UpdateCoachSkills))]
[DocExample(typeof(CreateCourse))]
[DocExample(typeof(UpdateCourseSkills))]
[DocExample(typeof(UpdateTimeSlots))]
[DocExample(typeof(UpdateCoachSkills))]
[DocExample(typeof(ConfirmCourse))]
[DocBoldHeader("Helpers")]
[DocExample(typeof(Skills))]
[DocReportHeader]
[DocReport]
public class HorsesForCoursesAcceptanceTests : WebrRunTest<HorsesForCoursesAcceptanceTests>
{
    protected override bool Asserts => false;
    protected override bool PassedExpectationsContains => false;
    protected override bool Report => true;
    protected override bool Explain => true;

    [Fact]
    public void Example() =>
        Document(a => a.Run(5.Runs(), 20.ExecutionsPerRun()), _ => { });

    [Fact]
    public void Debug() =>
        Webr.Named("Horses for Courses")
            .Context(() => new WebrApplicationFactory())
            .Client(a => a.CreateClient())
            .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
            .Reader(a => a.GetReader())
            .Scenario(
                new CreateCourse(),
                new UpdateCourseSkills());

    [Fact]
    public void Autopsy() =>
        GetWebr().Autopsy(1750761734, 20.ExecutionsPerRun(), AutopsyProbe.StartsWith("ActionShrinking"));

    [CodeSnippet]
    protected override ConfiguredCheckr GetWebr() =>
        Webr.Named("Horses for Courses")
            .Context(() => new WebrApplicationFactory())
            .Client(a => a.CreateClient())
            .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
            .Reader(a => a.GetReader())
            .Methods(
                new CreateCoach(),
                new UpdateCoachSkills(),
                new CreateCourse(),
                new UpdateCourseSkills(),
                new UpdateTimeSlots(),
                new AssignCoachToCourse(),
                new ConfirmCourse());
}

[CodeExample]
public class CreateCoach : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
         Create("Create Coach")
             .When<CoachInfo>(a => a.Count <= 10)
             .Route("/coaches")
             .Send(
                from name in Fuzzr.String()
                from email in Fuzzr.String()
                select new RegisterCoachRequest(name, email))
             .ResponseIs<int>(a => a > 0)
             .Store(a => new CoachInfo(a))
             .ReadBack((reader, info) => reader.Query(db => db.Find<Coach>(Id<Coach>.From(info.Id))))
             .Expect(
                 ("Name", (request, coach) => coach.Name.Value == request.Name),
                 ("Email", (request, coach) => coach.Email.Value == request.Email));
}

[CodeExample]
public class UpdateCoachSkills : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Update Coach Skills")
            .When<CoachInfo>(a => a.Count != 0)
            .Route(info => info.Id, a => $"/coaches/{a}/skills")
            .Send(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select new UpdateSkillsRequest([.. skills]))
            //, coach => Shrink.ValueFromState(coach.Skills, []))
            .Store((coach, request) => coach with { Skills = request.Skills })
            .ReadBack((reader, info) => reader.Query(db => db.Find<Coach>(Id<Coach>.From(info.Id))))
            .Expect(
                ("Skills", (request, coach) => Skills.Equal(request.Skills, coach.Skills)));
}

[CodeExample]
public class CreateCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Create("Create Course")
            .When<CourseInfo>(a => a.Count < 5)
            .Route("/courses")
            .Send(
                from name in Fuzzr.String()
                from startDate in Fuzzr.DateOnly(1.January(2026), 31.December(2026))
                from endDate in Fuzzr.DateOnly(startDate, startDate.AddDays(50))
                select new CreateCourseRequest(name, startDate, endDate))
            .ResponseIs<int>(a => a > 0)
            .Store(a => new CourseInfo(a))
            .ReadBack((reader, info) => reader.Query(db => db.Find<Course>(Id<Course>.From(info.Id))))
            .Expect(
                ("Name", (request, course) => course.Name.Value == request.Name),
                ("Start Date", (request, course) => course.Period.Start == request.StartDate),
                ("End Date", (request, course) => course.Period.End == request.EndDate));
}

[CodeExample]
public class UpdateCourseSkills : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Update Course Skills")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .Route(info => info.Id, a => $"/courses/{a}/skills")
            .Send(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select skills)
            .Store((info, request) => info)
            .ReadBack((reader, info) => reader.Query(db => db.Find<Course>(Id<Course>.From(info.Id))))
            .Trace("req", (info, req, course) => info)
            .Expect(("Skills", (request, course) => Skills.Equal(request, course.RequiredSkills)));
}

[CodeExample]
public class UpdateTimeSlots : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Update Time Slots")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .Route(info => info.Id, a => $"/courses/{a}/timeslots")
            .Send(TimeSlotsFuzzr)//, a => Shrink.Values(new TimeSlotRequest(CourseDay.Monday, 10, 11)))
            .Store((course, request) => course with { HasTimeSlots = true })
            .ReadBack((reader, info) => reader.Query(db => db.Find<Course>(Id<Course>.From(info.Id))))
            .Expect(("Timeslots", (request, course) => SlotsAreEqual(request, course.TimeSlots)));

    private static readonly FuzzrOf<List<TimeSlotRequest>> TimeSlotsFuzzr =
        from key in Fuzzr.Guid()
        from slots in TimeSlotFuzzr(key).Many(2, 5)
        select slots.ToList();

    private static FuzzrOf<TimeSlotRequest> TimeSlotFuzzr(object key) =>
        from day in Fuzzr.Enum<CourseDay>().Unique(key)
        from start in Fuzzr.Int(9, 17)
        from end in Fuzzr.Int(start + 1, 18)
        select new TimeSlotRequest(day, start, end);

    private static bool SlotsAreEqual(IEnumerable<TimeSlotRequest> expected, IEnumerable<TimeSlot> actual)
    {
        var expectedOrdered = expected.OrderBy(a => (a.Day, a.Start, a.End));
        var actualOrdered =
            actual.Select(a => new TimeSlotRequest(a.Day, a.Start.Value, a.End.Value))
                .OrderBy(a => (a.Day, a.Start, a.End));
        return expectedOrdered.SequenceEqual(actualOrdered);
    }
}

[CodeExample]
public class AssignCoachToCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("/courses/{id}/assign-coach")
            .When<CourseInfo, CoachInfo>(
                (course, coach) => course.IsConfirmed)// && IsSuitableFor(course, coach))
            .Route((course, coach) => course.Id, a => $"/courses/{a}/assign-coach")
            .Send((course, coach) => new AssignCoachRequest(coach.Id))
            .Store((course, coach, request) => course)
            .ReadBack((reader, course, coach) => reader.Query(db => (
                db.Find<Course>(Id<Course>.From(course.Id)),
                db.Find<Coach>(Id<Coach>.From(coach.Id)))))
            .Expect(
                ("Coach is assigned", (request, course, coach) =>
                    course.AssignedCoach == coach &&
                    coach.AssignedCourses.Contains(course)));

    // private static bool IsSuitableFor(IDbAccess db, CourseInfo courseInfo, CoachInfo coachInfo)
    // {
    //     var course = db.Query(ctx => ctx.Find<Course>(Id<Course>.From(courseInfo.Id))!);
    //     if (course == null) return false;
    //     if (course.AssignedCoach != null) return false;
    //     var coach = db.Query(ctx => ctx.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
    //     if (coach == null) return false;
    //     return coach.IsSuitableFor(course);
    // }
}

[CodeExample]
public class ConfirmCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Confirm Course")
            .When<CourseInfo>(info => info.HasTimeSlots && !info.IsConfirmed)
            .Route(a => a.Id, a => $"/courses/{a}/confirm")
            .Send()
            .Store((course) => course with { IsConfirmed = true })
            .ReadBack((reader, info) => reader.Query(db => db.Find<Course>(Id<Course>.From(info.Id))))
            .Expect(("Confirmed", (course) => course.IsConfirmed));
}


[CodeExample]
public static class Skills
{
    public static readonly string[] Pool =
    [
        "TDD", "Refactoring", "C#", "ASP.NET", "EF Core", "SQL",
        "DomainDrivenDesign", "UnitTesting", "Git", "CI/CD",
        "JavaScript", "React", "Elm", "Architecture"
    ];

    public static bool Equal(IEnumerable<string> expected, IEnumerable<Skill> actual)
    {
        var expectedOrdered = expected.Order();
        var actualOrdered = actual.Select(a => a.Value).Order();
        return expectedOrdered.SequenceEqual(actualOrdered);
    }
}