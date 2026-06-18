using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;
using QuickWebr.Tests.Docs.WalkThrough.Sub;
using QuickWebr.Tests.HorsesForCoursesTests;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Docs.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
> The final sales pitch.

You may have noticed the redundant looking call to `Observe` in the previous example.  
Maybe you thought it was the materialization method of the underlying builder infrastructure.

You are partially right, but ...

We can also define observations:. 
""")]
[DocExample(typeof(AssignedCoachesMustBeSuitable))]
[DocContent("And plug it in like so:")]
[DocExample(typeof(E_Invariants), nameof(RunWebr))]
[DocReportHeader]
[DocReport]
public class E_Invariants : WebrRunnerTest<E_Invariants>
{
    protected override bool Asserts => true;
    protected override bool PassedExpectationsContains => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public void Example()
        => Document(RunWebr, Verify);

    [CodeSnippet]
    [CodeRemove("A_InfrastructureSetup.")]
    [CodeRemove(".WithCustodian(TheJournalist)")]
    [CodeReplace("2093915464, 25.ExecutionsPerRun()", "5.Runs(), 50.ExecutionsPerRun()")]
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
            .Observe(new AssignedCoachesMustBeSuitable())
            .Run(2093915464, 25.ExecutionsPerRun());

    private void Verify(Article article)
    {
        Assert.Equal("All Assigned Coaches Must Be Suitable", article.FailureDescription());
        Assert.Equal("CoachInfo-2", article.FailingExpectationMessages()[0]);
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(8, article.Total().Executions());
        Assert.Equal(8, article.Total().Actions());
        Assert.Equal(5, article.Total().Inputs());
        Assert.Equal(8, article.Total().PoolTraces());
        Assert.Equal(9, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(20, article.Total().PassedExpectations());
        Assert.Equal(27, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create Course", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("'Create Course' Request", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Name: \"epmytod\", StartDate: 29.December(2026), EndDate: 26.January(2027) }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("'Create Course' to Pool", article.Execution(1).PoolTrace(1).Read().Label);
        Assert.Equal("CourseInfo-1", article.Execution(1).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("/courses", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(7, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Create Coach", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("'Create Coach' Request", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("{ Name: \"hvnszr\", Email: \"mer\" }", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("'Create Coach' to Pool", article.Execution(2).PoolTrace(1).Read().Label);
        Assert.Equal("CoachInfo-2", article.Execution(2).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("/coaches", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(10, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Update Course Skills", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("'Update Course Skills' Request", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("[ \"CI/CD\" ]", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Update Course Skills", article.Execution(3).PoolTrace(1).Read().Label);
        Assert.Equal("CourseInfo-1", article.Execution(3).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("/courses/1/skills", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal(11, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Update Time Slots", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("'Update Time Slots' Request", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("[ { Start: 13, End: 15 } ]", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Update Time Slots", article.Execution(4).PoolTrace(1).Read().Label);
        Assert.Equal("CourseInfo-1", article.Execution(4).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal("/courses/1/timeslots", article.Execution(4).Trace(1).Read().Value);
        Assert.Equal(12, article.Execution(5).Read().ExecutionId);
        Assert.Equal("Confirm Course", article.Execution(5).Action(1).Read().Label);
        Assert.Equal("Confirm Course", article.Execution(5).PoolTrace(1).Read().Label);
        Assert.Equal("CourseInfo-1", article.Execution(5).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(5).Trace(1).Read().Label);
        Assert.Equal("/courses/1/confirm", article.Execution(5).Trace(1).Read().Value);
        Assert.Equal(16, article.Execution(6).Read().ExecutionId);
        Assert.Equal("Update Coach Skills", article.Execution(6).Action(1).Read().Label);
        Assert.Equal("'Update Coach Skills' Request", article.Execution(6).Input(1).Read().Label);
        Assert.Equal("{ Skills: [ \"CI/CD\" ] }", article.Execution(6).Input(1).Read().Value);
        Assert.Equal("Update Coach Skills", article.Execution(6).PoolTrace(1).Read().Label);
        Assert.Equal("CoachInfo-2", article.Execution(6).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(6).Trace(1).Read().Label);
        Assert.Equal("/coaches/1/skills", article.Execution(6).Trace(1).Read().Value);
        Assert.Equal(18, article.Execution(7).Read().ExecutionId);
        Assert.Equal("Assign Coach to Course", article.Execution(7).Action(1).Read().Label);
        Assert.Equal("Assign Coach to Course", article.Execution(7).PoolTrace(1).Read().Label);
        Assert.Equal("( CourseInfo-1, CoachInfo-2 )", article.Execution(7).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(7).Trace(1).Read().Label);
        Assert.Equal("/courses/1/assign-coach", article.Execution(7).Trace(1).Read().Value);
        Assert.Equal("'Assign Coach to Course' Request", article.Execution(7).Trace(2).Read().Label);
        Assert.Equal("{ CoachId: 1 }", article.Execution(7).Trace(2).Read().Value);
        Assert.Equal(23, article.Execution(8).Read().ExecutionId);
        Assert.Equal("Update Coach Skills", article.Execution(8).Action(1).Read().Label);
        Assert.Equal("Update Coach Skills", article.Execution(8).PoolTrace(1).Read().Label);
        Assert.Equal("CoachInfo-2", article.Execution(8).PoolTrace(1).Read().Value);
        Assert.Equal("Route", article.Execution(8).Trace(1).Read().Label);
        Assert.Equal("/coaches/1/skills", article.Execution(8).Trace(1).Read().Value);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(8).Warning(1).Read().Value);
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
        Assert.Equal("'Create Coach' Status Code is Success", article.PassedExpectation(6).Read().Label);
        Assert.Equal(4, article.PassedExpectation(6).Read().TimesPassed);
        Assert.Equal("'Create Coach' Response", article.PassedExpectation(7).Read().Label);
        Assert.Equal(4, article.PassedExpectation(7).Read().TimesPassed);
        Assert.Equal("'Create Coach' Name", article.PassedExpectation(8).Read().Label);
        Assert.Equal(4, article.PassedExpectation(8).Read().TimesPassed);
        Assert.Equal("'Create Coach' Email", article.PassedExpectation(9).Read().Label);
        Assert.Equal(4, article.PassedExpectation(9).Read().TimesPassed);
        Assert.Equal("All Assigned Coaches Must Be Suitable", article.PassedExpectation(10).Read().Label);
        Assert.Equal(21, article.PassedExpectation(10).Read().TimesPassed);
        Assert.Equal("'Update Course Skills' Status Code is Success", article.PassedExpectation(11).Read().Label);
        Assert.Equal(3, article.PassedExpectation(11).Read().TimesPassed);
        Assert.Equal("'Update Course Skills' Skills", article.PassedExpectation(12).Read().Label);
        Assert.Equal(3, article.PassedExpectation(12).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Status Code is Success", article.PassedExpectation(13).Read().Label);
        Assert.Equal(7, article.PassedExpectation(13).Read().TimesPassed);
        Assert.Equal("'Update Coach Skills' Skills", article.PassedExpectation(14).Read().Label);
        Assert.Equal(7, article.PassedExpectation(14).Read().TimesPassed);
        Assert.Equal("'Update Time Slots' Status Code is Success", article.PassedExpectation(15).Read().Label);
        Assert.Equal(4, article.PassedExpectation(15).Read().TimesPassed);
        Assert.Equal("'Update Time Slots' Timeslots", article.PassedExpectation(16).Read().Label);
        Assert.Equal(4, article.PassedExpectation(16).Read().TimesPassed);
        Assert.Equal("'Confirm Course' Status Code is Success", article.PassedExpectation(17).Read().Label);
        Assert.Equal(2, article.PassedExpectation(17).Read().TimesPassed);
        Assert.Equal("'Confirm Course' Confirmed", article.PassedExpectation(18).Read().Label);
        Assert.Equal(2, article.PassedExpectation(18).Read().TimesPassed);
        Assert.Equal("'Assign Coach to Course' Status Code is Success", article.PassedExpectation(19).Read().Label);
        Assert.Equal(1, article.PassedExpectation(19).Read().TimesPassed);
        Assert.Equal("'Assign Coach to Course' Coach is assigned", article.PassedExpectation(20).Read().Label);
        Assert.Equal(1, article.PassedExpectation(20).Read().TimesPassed);
    }
}