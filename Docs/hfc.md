# ContactManager Acceptance Tests

**WebApplicationFactory:**  
```csharp
public class WebrApplicationFactory
    : WebApplicationFactory<HorsesForCourses.Api.Program>
{
    private SqliteConnection connection = null!;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.RemoveAll<AppDbContext>();
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connection));
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Auth:JwtKey"] = "a-very-long-random-secret-string-change-me",
                ["Auth:Issuer"] = "https://hfcc.example",
                ["Auth:Audience"] = "hfcc-api",
            };
            cfg.AddInMemoryCollection(overrides);
        });
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        connection.Dispose();
    }
    public EfReader GetReader() => new(Services);
}
```

**The Webr:**  
```csharp
Webr.Named("Horses for Courses")
    .Context(() => new WebrApplicationFactory())
    .Client(a => a.CreateClient())
    .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
    .Reader(a => a.GetReader())
    .Observe<CoachInfo>("All Assigned Coaches Must Be Suitable",
        (reader, coachInfo) =>
        {
            var coach = reader.Query(db => db.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
            return coach.AssignedCourses.All(course => coach.IsSuitableFor(course));
        })
    .Methods(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new UpdateTimeSlots(),
        new ConfirmCourse(),
        new AssignCoachToCourse());
```

**The Methods:**  
```csharp
public class CreateCoach : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
         Create("Create Coach")
             .When<CoachInfo>(a => a.Count <= 5)
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
```
```csharp
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
```
```csharp
public class CreateCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Create("Create Course")
            .When<CourseInfo>(a => a.Count < 3)
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
```
```csharp
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
```
```csharp
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
```
```csharp
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
```
```csharp
public class AssignCoachToCourse : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Assign Coach to Course")
            .When<CourseInfo, CoachInfo>(
                (reader, course, coach) => course.IsConfirmed && IsSuitableFor(reader, course, coach))
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
    private static bool IsSuitableFor(EfReader reader, CourseInfo courseInfo, CoachInfo coachInfo)
    {
        var course = reader.Query(ctx => ctx.Find<Course>(Id<Course>.From(courseInfo.Id))!);
        if (course == null) return false;
        if (course.AssignedCoach != null) return false;
        var coach = reader.Query(ctx => ctx.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
        if (coach == null) return false;
        return coach.IsSuitableFor(course);
    }
}
```

**Helpers:**  
```csharp
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
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                HorsesForCoursesAcceptanceTests.cs:45:1
 Original failing run:    48 executions
 Minimal failing case:    8 executions (after 52 shrinks)
 Seed:                    1211418307
 ------------------------------------------------------------
  Executed: Create Coach
   - 'Create Coach' to Pool = CoachInfo-1
   - 'Create Coach' Request = { Name: "uesayjtr", Email: "xm" }
   - Route                  = /coaches
 ------------------------------------------------------------
  Executed: Create Course
   - 'Create Course' to Pool = CourseInfo-1
   - 'Create Course' Request = { Name: "xydhkh", StartDate: 22.August(2026), EndDate: 17.September(2026) }
   - Route                   = /courses
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Update Course Skills           = CourseInfo-1
   - 'Update Course Skills' Request = [ "UnitTesting" ]
   - Route                          = /courses/1/skills
 ------------------------------------------------------------
  Executed: Update Time Slots
   - Update Time Slots           = CourseInfo-1
   - 'Update Time Slots' Request = [ { Start: 11, End: 12 } ]
   - Route                       = /courses/1/timeslots
 ------------------------------------------------------------
  Executed: Confirm Course
   - Confirm Course = CourseInfo-1
   - Route          = /courses/1/confirm
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Update Coach Skills           = CoachInfo-1
   - 'Update Coach Skills' Request = { Skills: [ "UnitTesting" ] }
   - Route                         = /coaches/1/skills
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - Assign Coach to Course           = ( CourseInfo-1, CoachInfo-1 )
   - 'Assign Coach to Course' Request = { CoachId: 1 }
   - Route                            = /courses/1/assign-coach
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Update Coach Skills = CoachInfo-1
   - Route               = /coaches/1/skills
   - WARNING: All inputs were considered irrelevant.
 ================================================================
  !! Expectation Failed: All Assigned Coaches Must Be Suitable
       CoachInfo-1
 ================================================================
 Passed Expectations
 - 'Create Coach' Status Code is Success: 6x
 - 'Create Coach' Response: 6x
 - 'Create Coach' Name: 6x
 - 'Create Coach' Email: 6x
 - All Assigned Coaches Must Be Suitable: 47x
 - 'Create Course' Status Code is Success: 3x
 - 'Create Course' Response: 3x
 - 'Create Course' Name: 3x
 - 'Create Course' Start Date: 3x
 - 'Create Course' End Date: 3x
 - 'Update Course Skills' Status Code is Success: 4x
 - 'Update Course Skills' Skills: 4x
 - 'Update Time Slots' Status Code is Success: 4x
 - 'Update Time Slots' Timeslots: 4x
 - 'Confirm Course' Status Code is Success: 3x
 - 'Confirm Course' Confirmed: 3x
 - 'Update Coach Skills' Status Code is Success: 27x
 - 'Update Coach Skills' Skills: 27x
 - 'Assign Coach to Course' Status Code is Success: 1x
 - 'Assign Coach to Course' Coach is assigned: 1x
 ------------------------------------------------------------
```
