using HorsesForCourses.Abstractions;
using HorsesForCourses.Api.Coaches;
using HorsesForCourses.Api.Courses;
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
[DocExample(typeof(ConfirmCourse))]
[DocExample(typeof(AssignCoachToCourse))]
[DocBoldHeader("Invariant")]
[DocExample(typeof(AssignedCoachesMustBeSuitable))]
[DocBoldHeader("Helpers")]
[DocExample(typeof(EfReader))]
[DocExample(typeof(Skills))]
[DocReportHeader]
[DocReport]
[DocHeader("Addendum: QuickCheckr features you get for free.")]
[DocBoldHeader("Scenarios")]
[DocExample(typeof(HorsesForCoursesAcceptanceTests), nameof(Scenario))]
[DocBoldHeader("Investigating")]
[DocExample(typeof(HorsesForCoursesAcceptanceTests), nameof(Conducting))]
[DocBoldHeader("Cold Cases (Vault)")]
[DocExample(typeof(HorsesForCoursesAcceptanceTests), nameof(ColdCases))]
[DocBoldHeader("Diagnostics")]
[DocExample(typeof(HorsesForCoursesAcceptanceTests), nameof(Autopsy))]
public class HorsesForCoursesAcceptanceTests : WebrTest<HorsesForCoursesAcceptanceTests>
{
    protected override bool Asserts => false;
    protected override bool PassedExpectationsContains => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact(Skip = "Explicit")]
    public void Example() =>
        Document(a => a.Run(1211418307, 50.ExecutionsPerRun()), _ => { });

    [Fact(Skip = "debug")]
    [CodeSnippet]
    public void Scenario() =>
        Webr.Named("Horses for Courses")
            .Context(() => new WebrApplicationFactory())
            .Client(a => a.CreateClient())
            .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
            .Reader(a => a.GetReader())
            .Scenario(
                new CreateCourse(),
                new UpdateCourseSkills());

    [Fact(Skip = "debug")]
    [CodeSnippet]
    public void Autopsy() =>
        GetWebr().Autopsy(1750761734, 20.ExecutionsPerRun(), AutopsyProbe.ActionShrinking);

    [Fact(Skip = "debug")]
    [CodeSnippet]
    public void Conducting() =>
        GetWebr().Conduct(5.Investigations(), 2.Runs(), 20.ExecutionsPerRun());

    [Fact(Skip = "debug")]
    [CodeSnippet]
    public void ColdCases() =>
        GetWebr().ReviewColdCases().Run(3.Runs(), 25.ExecutionsPerRun());

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
                new ConfirmCourse(),
                new AssignCoachToCourse())
            .Observe(new AssignedCoachesMustBeSuitable());
}

[CodeExample]
public class CreateCoach : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
         // The name of the Api Method
         // `Create` defaults to `HttpMethod.Post` and also defines the correct fluent pipeline
         Create("Create Coach")
             // Condition that decides whether or not this method can be run.
             // The generic type parameter defines the type of data the Webr uses to keep track of things (see below).
             .When<CoachInfo>(a => a.Count <= 3)
             // The route for this method.
             .Route("/coaches")
             // Perform the call using a random request.
             .Send(
                from name in Fuzzr.String()
                from email in Fuzzr.String()
                select new RegisterCoachRequest(name, email))
             // Simple check on the response from above call.
             .ResponseIs<int>(response => response > 0)
             // Store any info needed in order to drive and/or validate future method calls.
             .Store(response => new CoachInfo(response))
             // Retrieve data from the system for validation.
             .ReadBack((reader, info) => reader.Query(db => db.Find<Coach>(Id<Coach>.From(info.Id))))
             // Assert that the entity (Coach) is created correctly in the system.
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
                from skills in
                    Fuzzr
                        .OneOf(Skills.Pool)
                        .Unique(Guid.NewGuid())
                        .Many(3, 5)
                select new UpdateSkillsRequest([.. skills]))
            .Store((coach, request) => coach)
            .ReadBack((reader, info) => reader.Query(db => db.Find<Coach>(Id<Coach>.From(info.Id))))
            .Probe("Skills", (info, request, coach) => coach!)
            .Expect(
                ("Skills", (request, coach) => Skills.Equal(request.Skills, coach.Skills)));
}
//, coach => Shrink.ValueFromState(coach.Skills, []))


[CodeExample]
public class CreateCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Create("Create Course")
            .When<CourseInfo>(a => a.Count < 2)
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
public class AssignCoachToCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Assign Coach to Course")
            .When<CourseInfo, CoachInfo>(
                (reader, course, coach) => course.IsConfirmed && IsValidFor(reader, course, coach))
            .Route((course, coach) => course.Id, a => $"/courses/{a}/assign-coach")
            .Send((course, coach) => new AssignCoachRequest(coach.Id))
            .Store((course, coach, request) => course)
            .ReadBack((reader, course, coach) => reader.Query(db => (
                db.Find<Course>(Id<Course>.From(course.Id)),
                db.Find<Coach>(Id<Coach>.From(coach.Id)))))
            .Probe("Info", (p1, p2, r, e1, e2) => (p1.Id, p2.Id))
            .Probe("Entities", (p1, p2, r, e1, e2) => (e1!.RequiredSkills, e2!.Skills))
            .Expect(
                ("Coach is assigned", (request, course, coach) =>
                    course.AssignedCoach == coach &&
                    coach.AssignedCourses.Contains(course)));

    private static bool IsValidFor(EfReader reader, CourseInfo courseInfo, CoachInfo coachInfo)
    {
        var course = reader.Query(ctx => ctx.Find<Course>(Id<Course>.From(courseInfo.Id))!);
        if (course == null) return false;
        if (course.AssignedCoach != null) return false;
        var coach = reader.Query(ctx => ctx.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
        if (coach == null) return false;
        return coach.IsSuitableFor(course) && coach.IsAvailableFor(course);
    }
}

[CodeExample]
public class AssignedCoachesMustBeSuitable : Invariant<EfReader>
{
    public override Observation<EfReader> Define() =>
        Named("All Assigned Coaches Must Be Suitable")
            .ForAll<CoachInfo>((reader, coachInfo) =>
                {
                    var coach = reader.Query(db => db.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
                    return coach.AssignedCourses.All(course => coach.IsSuitableFor(course));
                });
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