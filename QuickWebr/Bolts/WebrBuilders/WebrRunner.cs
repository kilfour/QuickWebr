using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickWebr;

public class WebrRunner<TContext, TReader>
{
    private Func<CheckrConfig, CheckrConfig> configure;

    private WebrRunner(
        string name,
        Func<TContext> contextFactory,
        Func<TContext, HttpClient> clientFactory,
        Func<HttpClient, bool> isAuthenticated,
        Func<HttpClient, Task> authenticate,
        Func<TContext, TReader> readBackFactory
    )
    {
        configure = cfg => cfg with
        {
            FileAs = name,
            WarningLevel = WarningLevel.Debug,
            ShrinkMode = ShrinkMode.ExceptReduction
        };
        this.contextFactory = contextFactory;
        this.clientFactory = clientFactory;
        this.isAuthenticated = isAuthenticated;
        this.authenticate = authenticate;
        this.readBackFactory = readBackFactory;
    }

    public static WebrRunner<T, TReader> Named<T>(
        string name,
        Func<T> contextFactory,
        Func<T, HttpClient> clientFactory,
        Func<HttpClient, bool> isAuthenticated,
        Func<HttpClient, Task> authenticate,
        Func<T, TReader> readBackFactory) =>
        new(name, contextFactory, clientFactory, isAuthenticated, authenticate, readBackFactory);

    private ApiMethod<TReader>[] methods = [];
    public WebrRunner<TContext, TReader> Methods(params ApiMethod<TReader>[] methods) { this.methods = methods; return this; }

    //private readonly List<Func<IDbAccess, CheckrOf<Case>>> invariants = [];
    private readonly Func<TContext> contextFactory;
    private readonly Func<TContext, HttpClient> clientFactory;
    private readonly Func<HttpClient, bool> isAuthenticated;
    private readonly Func<HttpClient, Task> authenticate;
    private readonly Func<TContext, TReader> readBackFactory;

    // public WebrRunner<TContext, TReader> ForAll<T>(string label, Func<IDbAccess, T, bool> expectation)
    // {
    //     invariants.Add(a => Trackr.PoolExpectEach<T>(label, b => expectation(a, b)));
    //     return this;
    // }

    // public Webr<TContext> DisableWarnings()
    // {
    //     var previous = configure;
    //     configure = cfg =>
    //     {
    //         var configured = previous(cfg);
    //         return configured with { ReportMode = configured.ReportMode };
    //     };
    //     return this;
    // }

    public void Run(RunCount runs, ExecutionCount executionsPerRun) =>
        TheCheckr().Run(runs, executionsPerRun, configure);

    public void Run(int seed, ExecutionCount executionsPerRun) =>
        TheCheckr().Run(seed, executionsPerRun, configure);

    public void Autopsy(int seed, ExecutionCount executionsPerRun, Microtome? microtome = null) =>
        TheCheckr().Autopsy(seed, executionsPerRun, a => configure(a), microtome is null ? Microtome.Default : microtome);

    private CheckrOf<Case> TheCheckr() =>
        from context in Trackr.Stashed(() => contextFactory())
        from client in Trackr.Stashed(() => clientFactory(context))
        from db in Trackr.Stashed(() => readBackFactory(context))
        from auth in Checkr.ActWhen("Auth", () => !isAuthenticated(client), () => authenticate(client))
        from _ in Checkr.OneOfWhen([.. methods.Select(m => m.Define().Checkr(client, db))])
            // from invariants in Combine.Checkrs(invariants.Select(a => a(api)))
        select Case.Closed;
}
