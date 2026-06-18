using HorsesForCourses.Abstractions;
using HorsesForCourses.Api.Coaches;
using HorsesForCourses.Domain.Coaches;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickWebr.Tests.HorsesForCoursesTests;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Docs.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
So now we can define `ApiMethod` derived classes representing calls to our Api. 

Here's an annotated example:
""")]
[DocExample(typeof(CreateCoachAnnotated))]
[DocContent("Which we can run like so:")]
[DocExample(typeof(B_TheFirstApiMethod), nameof(RunWebr))]
[DocReportHeader]
[DocReport]
public class B_TheFirstApiMethod : WebrRunnerTest<B_TheFirstApiMethod>
{
    protected override bool Asserts => false;
    protected override bool PassedExpectationsContains => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public void Example()
        => Document(RunWebr, Verify);

    [CodeSnippet]
    [CodeRemove("A_InfrastructureSetup.")]
    [CodeRemove(".WithCustodian(TheJournalist)")]
    private void RunWebr() =>
        // From Infrastructure Setup.
        A_InfrastructureSetup.WebrRunner().WithCustodian(TheJournalist)
            .Scenario(new CreateCoachAnnotated());

    private void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(5, article.Total().PassedExpectations());
        Assert.Equal("CreateCoachAnnotated Can Execute", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("'Create Coach' Status Code is Success", article.PassedExpectation(2).Read().Label);
        Assert.Equal(1, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("'Create Coach' Response", article.PassedExpectation(3).Read().Label);
        Assert.Equal(1, article.PassedExpectation(3).Read().TimesPassed);
        Assert.Equal("'Create Coach' Name", article.PassedExpectation(4).Read().Label);
        Assert.Equal(1, article.PassedExpectation(4).Read().TimesPassed);
        Assert.Equal("'Create Coach' Email", article.PassedExpectation(5).Read().Label);
        Assert.Equal(1, article.PassedExpectation(5).Read().TimesPassed);
    }


    [CodeExample]
    [CodeRemove("Annotated")]
    public class CreateCoachAnnotated : ApiMethod<EfReader>
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
}