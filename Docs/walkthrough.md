## Walk Through
### Infrastructure Setup
First things first.  
We need to go through the usual WebApi acceptance test setup ceremony.  
In this one we swap out the configured DbContext with an in-memory sqlite one.  
And we add a method that returns a `IReader`, here an `EfReader`, which we can use for verifying things later.  
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
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
            }
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
                ["Auth:JwtKey"] = "a-very-long-random-secret-string",
                ["Auth:Issuer"] = "https://hfc.example",
                ["Auth:Audience"] = "hfc-api",
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
```csharp
public class EfReader(IServiceProvider services)
{
    public T Query<T>(Func<AppDbContext, T> query)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return query(db);
    }
}
```

**The Runner:**  
Once that is out of the way, we can set up a method that returns a reusable `WebrRunner`.  
```csharp
public static WebrRunner<WebrApplicationFactory, EfReader> WebrRunner() =>
    Webr.Named("Horses for Courses")
        .Context(() => new WebrApplicationFactory())
        .Client(a => a.CreateClient())
        .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
        .Reader(a => a.GetReader());
```
### The First Api Method
So now we can define `ApiMethod` derived classes representing calls to our Api. 

Here's an annotated example:  
```csharp
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
```
Which we can run like so:  
```csharp
// From Infrastructure Setup.
WebrRunner()
    .Scenario(new CreateCoach());
```

**The Report:**  
```text
------------------------------------------------------------
 1 Run
 ------------------------------------------------------------
 Passed Expectations
 - CreateCoach Can Execute: 1x
 - 'Create Coach' Status Code is Success: 1x
 - 'Create Coach' Response: 1x
 - 'Create Coach' Name: 1x
 - 'Create Coach' Email: 1x
 ------------------------------------------------------------
```
### Another Method
Up to this point we have not really gained much.  
We added a dependency on a new library, incurring the cost of getting to know the lingo,
and although I personally kind of like it, we have not really simplified things greatly.

But here's the first win.  
Which I think you would have seen coming just by looking at the method name of `Scenario`.

After *Create* comes *Update*, and we can reuse the `CreateCoach` *specification* to run another scenario.

Adding another Api Method:  
```csharp
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
```
Running the new scenario:  
```csharp
WebrRunner()
    .Scenario(
        new CreateCoach(),
        new UpdateCoachSkills());
```

**The Report:**  
```text
------------------------------------------------------------
 1 Run
 ------------------------------------------------------------
 Passed Expectations
 - CreateCoach Can Execute: 1x
 - 'Create Coach' Status Code is Success: 1x
 - 'Create Coach' Response: 1x
 - 'Create Coach' Name: 1x
 - 'Create Coach' Email: 1x
 - UpdateCoachSkills Can Execute: 1x
 - 'Update Coach Skills' Status Code is Success: 1x
 - 'Update Coach Skills' Skills: 1x
 ------------------------------------------------------------
```
### But That Gets Out Of Hand Quickly
Indeed it does.  
For simple scenarios the previous examples work fine,
but most Apis contain a lot more methods and writing out all possible scenarios gets very tedious, very quickly.

The simple 'Horses for Courses' Api deals with Coaches and Courses,
two related entities and already consists of 7 calls. 

QuickWebr however, allows you to explore all possible scenarios by using `.Methods(...)` instead of `.Scenario(...)`  
```csharp
WebrRunner()
    .Methods(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new UpdateTimeSlots(),
        new ConfirmCourse(),
        new AssignCoachToCourse())
    .Observe()
    .Run(1.Runs(), 20.ExecutionsPerRun());
```

**The Report:**  
```text
------------------------------------------------------------
 1 Run
 ------------------------------------------------------------
 Passed Expectations
 - 'Create Course' Status Code is Success: 2x
 - 'Create Course' Response: 2x
 - 'Create Course' Name: 2x
 - 'Create Course' Start Date: 2x
 - 'Create Course' End Date: 2x
 - 'Update Time Slots' Status Code is Success: 2x
 - 'Update Time Slots' Timeslots: 2x
 - 'Confirm Course' Status Code is Success: 2x
 - 'Confirm Course' Confirmed: 2x
 - 'Update Course Skills' Status Code is Success: 5x
 - 'Update Course Skills' Skills: 5x
 - 'Create Coach' Status Code is Success: 4x
 - 'Create Coach' Response: 4x
 - 'Create Coach' Name: 4x
 - 'Create Coach' Email: 4x
 - 'Assign Coach to Course' Status Code is Success: 1x
 - 'Assign Coach to Course' Coach is assigned: 1x
 - 'Update Coach Skills' Status Code is Success: 4x
 - 'Update Coach Skills' Skills: 4x
 ------------------------------------------------------------
```
### Invariants
> The final sales pitch.

You may have noticed the redundant looking call to `Observe` in the previous example.  
Maybe you thought it was the materialization method of the underlying builder infrastructure.

You are partially right, but ...

We can also define observations:
```csharp
public class AssignedCoachesMustBeSuitable : Invariant<EfReader>
{
    public override Observation<EfReader> Define() =>
        Named("All Assigned Coaches Must Be Suitable")
            .ForAll<CoachInfo>((reader, coachInfo) =>
                {
                    var all = reader.Query(
                        db => db.Set<Coach>().Include(a => a.AssignedCourses)
                        .Single(a => a.Id == Id<Coach>.From(coachInfo.Id)));
                    // all.PulseToLog("debug.log");
                    var coach = reader.Query(db => db.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
                    return coach.AssignedCourses.All(course => coach.IsSuitableFor(course));
                });
}
```
And plug it in like so:  
```csharp
WebrRunner()
    .Methods(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new UpdateTimeSlots(),
        new ConfirmCourse(),
        new AssignCoachToCourse())
.Observe(new AssignedCoachesMustBeSuitable())
.Run(5.Runs(), 50.ExecutionsPerRun());
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_Invariants.cs:45:1
 Original failing run:    23 executions
 Minimal failing case:    8 executions (after 27 shrinks)
 Seed:                    2093915464
 ------------------------------------------------------------
  Executed: Create Course
   - 'Create Course' to Pool = CourseInfo-1
   - 'Create Course' Request = { Name: "epmytod", StartDate: 29.December(2026), EndDate: 26.January(2027) }
   - Route                   = /courses
 ------------------------------------------------------------
  Executed: Create Coach
   - 'Create Coach' to Pool = CoachInfo-2
   - 'Create Coach' Request = { Name: "hvnszr", Email: "mer" }
   - Route                  = /coaches
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Update Course Skills           = CourseInfo-1
   - 'Update Course Skills' Request = [ "CI/CD" ]
   - Route                          = /courses/1/skills
 ------------------------------------------------------------
  Executed: Update Time Slots
   - Update Time Slots           = CourseInfo-1
   - 'Update Time Slots' Request = [ { Start: 13, End: 15 } ]
   - Route                       = /courses/1/timeslots
 ------------------------------------------------------------
  Executed: Confirm Course
   - Confirm Course = CourseInfo-1
   - Route          = /courses/1/confirm
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Update Coach Skills           = CoachInfo-2
   - 'Update Coach Skills' Request = { Skills: [ "CI/CD" ] }
   - Route                         = /coaches/1/skills
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - Assign Coach to Course           = ( CourseInfo-1, CoachInfo-2 )
   - Route                            = /courses/1/assign-coach
   - 'Assign Coach to Course' Request = { CoachId: 1 }
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Update Coach Skills = CoachInfo-2
   - Route               = /coaches/1/skills
   - WARNING: All inputs were considered irrelevant.
 ================================================================
  !! Expectation Failed: All Assigned Coaches Must Be Suitable
       CoachInfo-2
 ================================================================
 Passed Expectations
 - 'Create Course' Status Code is Success: 2x
 - 'Create Course' Response: 2x
 - 'Create Course' Name: 2x
 - 'Create Course' Start Date: 2x
 - 'Create Course' End Date: 2x
 - 'Create Coach' Status Code is Success: 4x
 - 'Create Coach' Response: 4x
 - 'Create Coach' Name: 4x
 - 'Create Coach' Email: 4x
 - All Assigned Coaches Must Be Suitable: 21x
 - 'Update Course Skills' Status Code is Success: 3x
 - 'Update Course Skills' Skills: 3x
 - 'Update Coach Skills' Status Code is Success: 7x
 - 'Update Coach Skills' Skills: 7x
 - 'Update Time Slots' Status Code is Success: 4x
 - 'Update Time Slots' Timeslots: 4x
 - 'Confirm Course' Status Code is Success: 2x
 - 'Confirm Course' Confirmed: 2x
 - 'Assign Coach to Course' Status Code is Success: 1x
 - 'Assign Coach to Course' Coach is assigned: 1x
 ------------------------------------------------------------
```
