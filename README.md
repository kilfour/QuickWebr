# <img src='icon.png' width='40' align='top'/> QuickWebr
> When your bugs are from Mars.

Property-based testing for the pragmatic.  

[![Docs](https://img.shields.io/badge/docs-QuickWebr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickWebr/blob/main/Docs/walkthrough.md)
[![NuGet](https://img.shields.io/nuget/v/QuickWebr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickWebr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickWebr/blob/main/LICENSE)


**QuickWebr** brings stateful testing to ASP.NET Web APIs.

It lets you describe API calls as reusable `ApiMethod`s, then run them either as explicit scenarios or as randomly explored sequences of valid operations.

Instead of writing one test for every possible path through your API, you describe:

* When an endpoint can be called.
* How to build its request.
* Which route to use.
* What information should be remembered.
* How to read the system back.
* What expectations must hold.

QuickWebr then explores combinations of those calls and shrinks failures to small, reproducible API stories.

## Example

```csharp
public class CreateContact : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Create("Create Contact")
            .Always<ContactInfo>()
            .Route("api/contacts")
            .Send(Fuzzr.One<CreateContactRequest>())
            .ResponseIs<CreateContactResponse>(a => a is not null)
            .Store(response => new ContactInfo(response.Id, response.Name))
            .ReadBack((repo, info) => repo.GetById(info.Id))
            .Expect(("Name", (request, contact) => contact.Name == request.Name));
}
```

A Webr can run methods in a fixed scenario:

```csharp
Webr.Named("Contact Manager")
    .Context(() => new WebrApplicationFactory())
    .Client(a => a.CreateClient())
    .Reader(a => a.GetReader())
    .Scenario(
        new CreateContact(),
        new UpdateContact());
```

Or explore all supplied methods as a stateful API surface:

```csharp
Webr.Named("Contact Manager")
    .Context(() => new WebrApplicationFactory())
    .Client(a => a.CreateClient())
    .Reader(a => a.GetReader())
    .Methods(
        new CreateContact(),
        new UpdateContact(),
        new DeleteContact(),
        new GetContacts(),
        new SearchContacts())
    .Observe();
```

## Invariants

QuickWebr can also observe rules that should remain true after API calls have been executed.

```csharp
public class AssignedCoachesMustBeSuitable : Invariant<EfReader>
{
    public override Observation<EfReader> Define() =>
        Named("All Assigned Coaches Must Be Suitable")
            .ForAll<CoachInfo>((reader, coachInfo) =>
            {
                var coach = reader.Query(db =>
                    db.Set<Coach>()
                        .Include(a => a.AssignedCourses)
                        .Single(a => a.Id == Id<Coach>.From(coachInfo.Id)));
                return coach.AssignedCourses.All(course => coach.IsSuitableFor(course));
            });
}
```

```csharp
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
    .Observe(new AssignedCoachesMustBeSuitable())
    .Run(5.Runs(), 50.ExecutionsPerRun());
```

When an invariant fails, QuickWebr reports the smallest API story it could find that still reproduces the problem.

## Highlights

* **Scenario testing:** Run API methods in a fixed, explicit order.
* **Stateful exploration:** Let QuickWebr explore valid sequences of API calls.
* **Shrinking:** Reduce failing API histories to small, reproducible reports.
* **Read-back validation:** Verify behaviour against the real system state.
* **Invariants:** Check business rules across the whole explored API history.
* **QuickCheckr powered:** Seeds, investigations, cold cases, diagnostics, and shrinking come along for free.

## Installation

QuickWebr is available on NuGet:

```bash
Install-Package QuickWebr
```

Or via the .NET CLI:

```bash
dotnet add package QuickWebr
```

## Documentation

QuickWebr is young, but the examples are executable and focused on real ASP.NET API testing.

Start with the [walkthrough](https://github.com/kilfour/QuickWebr/blob/main/Docs/walkthrough.md), then look at the small [Contact Manager](https://github.com/kilfour/QuickWebr/blob/main/Docs/contact-manager.md) example and the larger [Horses for Courses](https://github.com/kilfour/QuickWebr/blob/main/Docs/hfc.md) acceptance tests.

## Dependencies

* **QuickCheckr**: Stateful checking, shrinking, reporting, seeds, investigations, and diagnostics.
* **QuickFuzzr**: Random request and input generation.
* **QuickPulse** / **QuickPulse.Show**: Reporting, diagnostics, and value display.

## License
This project is licensed under the [MIT License](https://github.com/kilfour/QuickWebr/blob/main/LICENSE).
