using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickWebr.Bolts.WebrBuilders;




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
    public ConfiguredCheckr Methods(params ApiMethod<TReader>[] methods)
    {
        this.methods = methods;
        return TheCheckr().Configure(configure);
    }

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

    private CheckrOf<Case> TheCheckr() =>
        from context in Trackr.Stashed(() => contextFactory())
        from client in Trackr.Stashed(() => clientFactory(context))
        from db in Trackr.Stashed(() => readBackFactory(context))
        from auth in Checkr.ActWhen("Auth", () => !isAuthenticated(client), () => authenticate(client))
        from _ in Checkr.OneOfWhen([.. methods.Select(m => m.Define().Checkr(client, db))])
            // from invariants in Combine.Checkrs(invariants.Select(a => a(api)))
        select Case.Closed;

    public void Scenario(params ApiMethod<TReader>[] methods)
    {
        var checkr =
            from context in Trackr.Stashed(() => contextFactory())
            from client in Trackr.Stashed(() => clientFactory(context))
            from db in Trackr.Stashed(() => readBackFactory(context))
            from auth in Checkr.ActWhen("Auth", () => !isAuthenticated(client), () => authenticate(client))
            from seq in Checkr.Sequence(
                [.. methods.Select(m => (m.GetType().Name, Option: m.Define().Checkr(client, db)))
                    .Select(a =>
                    from option in a.Option
                    from checkExecute in Checkr.Expect($"{a.Name} Can Execute", () => option.Item1())
                    from execute in option.Item2
                    select Case.Closed)]
            )
            select Case.Closed;
        checkr.Configure(configure).Run(1.Runs(), methods.Length.ExecutionsPerRun());
    }
}
