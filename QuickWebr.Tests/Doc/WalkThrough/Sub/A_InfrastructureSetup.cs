using QuickCheckr.Authoring;
using QuickPulse.Explains;
using QuickWebr.Bolts.WebrBuilders;
using QuickWebr.Tests.HorsesForCoursesTests;

namespace QuickWebr.Tests.Doc.WalkThrough.Sub;

[DocFile]
[DocContent(
"""
First things first.  
We need to go through the usual WebApi acceptance test setup ceremony.  
In this one we swap out the configured DbContext with an inmemory sqlite one.  
And we add a method that returns a `IReader`, here an `EfReader`, which we can use for verifying things later.
""")]
[DocExample(typeof(WebrApplicationFactory))]
[DocExample(typeof(EfReader))]
[DocContent(
"""
Once that is out of the way, we can set up a method that returns a reusable `WebrRunner`.
""")]
[DocExample(typeof(A_InfrastructureSetup), nameof(WebrRunner))]
public class A_InfrastructureSetup : QuickCheckrTest<A_InfrastructureSetup>
{
    [CodeExample]
    public static WebrRunner<WebrApplicationFactory, EfReader> WebrRunner() =>
        Webr.Named("Horses for Courses")
            .Context(() => new WebrApplicationFactory())
            .Client(a => a.CreateClient())
            .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
            .Reader(a => a.GetReader());
}