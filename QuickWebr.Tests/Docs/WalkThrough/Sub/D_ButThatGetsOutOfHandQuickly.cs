using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;
using QuickWebr.Tests.HorsesForCoursesTests;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Docs.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
Indeed it does.  
For simple scenarios the previous examples work fine,
but most Apis contain a lot more methods and writing out all possible scenarios gets very tedious, very quickly.

The simple 'Horses for Courses' Api deals with Coaches and Courses,
two related entities and already consists of 7 calls. 

QuickWebr however, allows you to explore all possible scenarios by using `.Methods(...)` instead of `.Scenario(...)`
""")]
[DocExample(typeof(D_ButThatGetsOutOfHandQuickly), nameof(RunWebr))]
[DocContent("`.Observe()` turns the selected API methods into an executable *Checkr*. We'll revisit this later.")]
[DocReportHeader]
[DocReport]
public class D_ButThatGetsOutOfHandQuickly : WebrRunnerTest<D_ButThatGetsOutOfHandQuickly>
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
    [CodeReplace("1234567898", "1.Runs()")]
    private void RunWebr() =>
        A_InfrastructureSetup.WebrRunner().WithCustodian(TheJournalist)
            .Methods(
                new CreateCoach(),
                new UpdateCoachSkills(),
                new CreateCourse(),
                new UpdateCourseSkills(),
                new UpdateTimeSlots(),
                new ConfirmCourse(),
                new AssignCoachToCourse())
            .Observe()
            .Run(1234567898, 20.ExecutionsPerRun());

    private void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(19, article.Total().PassedExpectations());
        Assert.Equal("'Create Course' Status Code is Success", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("'Create Course' Response", article.PassedExpectation(2).Read().Label);
        Assert.Equal(2, article.PassedExpectation(2).Read().TimesPassed);
        Assert.Equal("'Create Course' Name", article.PassedExpectation(3).Read().Label);
        Assert.Equal(2, article.PassedExpectation(3).Read().TimesPassed);
        Assert.Equal("'Create Course' Start Date", article.PassedExpectation(4).Read().Label);
        Assert.Equal(2, article.PassedExpectation(4).Read().TimesPassed);
        Assert.Equal("'Create Course' End Date", article.PassedExpectation(5).Read().Label);
        Assert.Equal(2, article.PassedExpectation(5).Read().TimesPassed);
        Assert.Equal("'Update Time Slots' Status Code is Success", article.PassedExpectation(6).Read().Label);
        Assert.Equal(2, article.PassedExpectation(6).Read().TimesPassed);
        Assert.Equal("'Update Time Slots' Timeslots", article.PassedExpectation(7).Read().Label);
        Assert.Equal(2, article.PassedExpectation(7).Read().TimesPassed);
        Assert.Equal("'Confirm Course' Status Code is Success", article.PassedExpectation(8).Read().Label);
        Assert.Equal(2, article.PassedExpectation(8).Read().TimesPassed);
        Assert.Equal("'Confirm Course' Confirmed", article.PassedExpectation(9).Read().Label);
        Assert.Equal(2, article.PassedExpectation(9).Read().TimesPassed);
        Assert.Equal("'Update Course Skills' Status Code is Success", article.PassedExpectation(10).Read().Label);
        Assert.Equal(5, article.PassedExpectation(10).Read().TimesPassed);
        Assert.Equal("'Update Course Skills' Skills", article.PassedExpectation(11).Read().Label);
        Assert.Equal(5, article.PassedExpectation(11).Read().TimesPassed);
        Assert.Equal("'Create Coach' Status Code is Success", article.PassedExpectation(12).Read().Label);
        Assert.Equal(4, article.PassedExpectation(12).Read().TimesPassed);
        Assert.Equal("'Create Coach' Response", article.PassedExpectation(13).Read().Label);
        Assert.Equal(4, article.PassedExpectation(13).Read().TimesPassed);
        Assert.Equal("'Create Coach' Name", article.PassedExpectation(14).Read().Label);
        Assert.Equal(4, article.PassedExpectation(14).Read().TimesPassed);
        Assert.Equal("'Create Coach' Email", article.PassedExpectation(15).Read().Label);
        Assert.Equal(4, article.PassedExpectation(15).Read().TimesPassed);
        Assert.Equal("'Assign Coach to Course' Status Code is Success", article.PassedExpectation(16).Read().Label);
        Assert.Equal(1, article.PassedExpectation(16).Read().TimesPassed);
        Assert.Equal("'Assign Coach to Course' Coach is assigned", article.PassedExpectation(17).Read().Label);
        Assert.Equal(1, article.PassedExpectation(17).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Status Code is Success", article.PassedExpectation(18).Read().Label);
        Assert.Equal(4, article.PassedExpectation(18).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Skills", article.PassedExpectation(19).Read().Label);
        Assert.Equal(4, article.PassedExpectation(19).Read().TimesPassed);
    }
}