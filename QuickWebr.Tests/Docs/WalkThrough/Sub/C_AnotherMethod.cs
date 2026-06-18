using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;
using QuickWebr.Tests.HorsesForCoursesTests;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Docs.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
Up to this point we have not really gained much.  
We added a dependency on a new library, incurring the cost of getting to know the lingo,
and although I personally kind of like it, we have not really simplified things greatly.

But here's the first win.  
Which I think you would have seen coming just by looking at the method name of `Scenario`.

After *Create* comes *Update*, and we can reuse the `CreateCoach` *specification* to run another scenario.

Adding another Api Method:
""")]
[DocExample(typeof(UpdateCoachSkills))]
[DocContent("Running the new scenario:")]
[DocExample(typeof(C_AnotherMethod), nameof(RunWebr))]
[DocReportHeader]
[DocReport]
public class C_AnotherMethod : WebrRunnerTest<C_AnotherMethod>
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
        A_InfrastructureSetup.WebrRunner().WithCustodian(TheJournalist)
            .Scenario(
                new CreateCoach(),
                new UpdateCoachSkills());

    private void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(8, article.Total().PassedExpectations());
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
        Assert.Equal("UpdateCoachSkills Can Execute", article.PassedExpectation(6).Read().Label);
        Assert.Equal(1, article.PassedExpectation(6).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Status Code is Success", article.PassedExpectation(7).Read().Label);
        Assert.Equal(1, article.PassedExpectation(7).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Skills", article.PassedExpectation(8).Read().Label);
        Assert.Equal(1, article.PassedExpectation(8).Read().TimesPassed);
    }
}