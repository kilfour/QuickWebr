using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;
using QuickWebr.Tests.Docs.WalkThrough.Sub;
using QuickWebr.Tests.HorsesForCoursesTests;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Docs.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
So now we can define `ApiMethod` derived classes representing calls to our Api. 

Here's an annotated example:
""")]
[DocExample(typeof(CreateCoach))]
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
            .Scenario(new CreateCoach());

    private void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(5, article.Total().PassedExpectations());
        Assert.Equal("CreateCoach Can Execute", article.PassedExpectation(1).Read().Label);
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
}