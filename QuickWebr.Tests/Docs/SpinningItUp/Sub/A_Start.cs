using QuickPulse.Explains;
using QuickWebr.Tests.Tools;

namespace QuickWebr.Tests.Docs.SpinningItUp.Sub;

//[DocFile]
public class A_Start : WebrRunnerTest<A_Start>
{
    protected override bool Asserts => false;
    protected override bool PassedExpectationsContains => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    // [Fact]
    // public void Example()
    //     => Document(RunWebr, Verify);

    // [CodeSnippet]
    // [CodeRemove("A_InfrastructureSetup.")]
    // [CodeRemove(".WithCustodian(TheJournalist)")]
    // private void RunWebr() =>
    //     A_InfrastructureSetup.WebrRunner().WithCustodian(TheJournalist)
    //         .Scenario(
    //             new CreateCoach(),
    //             new UpdateCoachSkills());

    // private void Verify(Article article)
    // {
    // }
}