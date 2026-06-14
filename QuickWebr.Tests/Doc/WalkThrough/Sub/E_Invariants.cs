using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;
using QuickWebr.Tests.HorsesForCoursesTests;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Doc.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
> The final sales pitch.

You may have noticed the redundant looking call to `Observe` in the previous example.  
Maybe you thought it was the materialization method of the underlying builder infrastructure.

You are partially right, but ...

We can also define `Observations`. 
""")]
[DocExample(typeof(AssignedCoachesMustBeSuitable))]
[DocContent("And plug it in like so:")]
[DocExample(typeof(E_Invariants), nameof(RunWebr))]
[DocReportHeader]
[DocReport]
public class E_Invariants : WebrRunnerTest<E_Invariants>
{
    protected override bool Asserts => false;
    protected override bool PassedExpectationsContains => false;
    protected override bool Report => true;
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
    .Autopsy(2093915464, 25.ExecutionsPerRun(),
        AutopsyProbe.ExecutionShrinking.BreakOn(
            a => AutopsyProbe.Prepare(a).StartsWith("ExecutionShrinking.Pass-1.Checking-2.Executing-18")));
    //.Run(500.Runs(), 25.ExecutionsPerRun());

    private void Verify(Article article)
    {

    }
}