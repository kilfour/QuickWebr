using Microsoft.EntityFrameworkCore;
using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickWebr;



public static class Webr
{
    public static WebrBuilder Named(string name) => new(name);

    public class WebrBuilder(string name)
    {
        public WebrBuilder1<T> Context<T>(Func<T> contextFactory) => new(name, contextFactory);
    }

    public class WebrBuilder1<T>(string name, Func<T> contextFactory)
    {
        public WebrBuilder2<T> Client(Func<T, HttpClient> clientFactory) => new(name, contextFactory, clientFactory);
    }

    public class WebrBuilder2<T>(string name, Func<T> contextFactory, Func<T, HttpClient> clientFactory)
    {
        private Func<HttpClient, bool> isAuthenticated = a => true;
        private Func<HttpClient, Task> authenticate = a => Task.CompletedTask;

        public WebrBuilder2<T> Authentication(Func<HttpClient, bool> isAuthenticated, Func<HttpClient, Task> authenticate)
        {
            this.isAuthenticated = isAuthenticated;
            this.authenticate = authenticate;
            return this;
        }

        public Webr<T> Database(Func<T, DbContext> dbFactory) =>
            Webr<T>.Named(name, contextFactory, clientFactory, isAuthenticated, authenticate, dbFactory);
    }
}

public class Webr<TContext>
{
    private Func<CheckrConfig, CheckrConfig> configure;

    private Webr(
        string name,
        Func<TContext> contextFactory,
        Func<TContext, HttpClient> clientFactory,
        Func<HttpClient, bool> isAuthenticated,
        Func<HttpClient, Task> authenticate,
        Func<TContext, DbContext> dbFactory
    )
    {
        configure = cfg => cfg with { FileAs = name };
        this.contextFactory = contextFactory;
        this.clientFactory = clientFactory;
        this.isAuthenticated = isAuthenticated;
        this.authenticate = authenticate;
        this.dbFactory = dbFactory;
    }

    public static Webr<T> Named<T>(
        string name,
        Func<T> contextFactory,
        Func<T, HttpClient> clientFactory,
        Func<HttpClient, bool> isAuthenticated,
        Func<HttpClient, Task> authenticate,
        Func<T, DbContext> dbFactory) =>
        new(name, contextFactory, clientFactory, isAuthenticated, authenticate, dbFactory);

    private ApiMethod[] methods = [];
    public Webr<TContext> Methods(params ApiMethod[] methods) { this.methods = methods; return this; }

    private readonly List<CheckrOf<Case>> pools = [];
    public Webr<TContext> PoolOf<T>()
    {
        pools.Add(Trackr.PoolFor<T>());
        return this;
    }

    private readonly List<Func<IDbAccess, CheckrOf<Case>>> invariants = [];
    private readonly Func<TContext> contextFactory;
    private readonly Func<TContext, HttpClient> clientFactory;
    private readonly Func<HttpClient, bool> isAuthenticated;
    private readonly Func<HttpClient, Task> authenticate;
    private readonly Func<TContext, DbContext> dbFactory;

    public Webr<TContext> ForAll<T>(string label, Func<IDbAccess, T, bool> expectation)
    {
        invariants.Add(a => Trackr.PoolExpectEach<T>(label, b => expectation(a, b)));
        return this;
    }

    public Webr<TContext> DisableWarnings()
    {
        var previous = configure;
        configure = cfg =>
        {
            var configured = previous(cfg);
            return configured with { ReportMode = configured.ReportMode & ~ReportMode.Warning };
        };
        return this;
    }

    public void Run(CheckrOfTRun.RunCount runs, CheckrOfTRun.ExecutionCount executionsPerRun) =>
        TheCheckr().Run(runs, executionsPerRun, configure);

    public void Run(int seed, CheckrOfTRun.ExecutionCount executionsPerRun) =>
        TheCheckr().Run(seed, executionsPerRun, configure);

    private CheckrOf<Case> TheCheckr() =>
        from context in Trackr.Stashed(() => contextFactory())
        from api in Trackr.Stashed(() => new Spider(clientFactory(context), () => dbFactory(context)))
        from pools in Combine.Checkrs(pools)
        from auth in Checkr.ActWhen("Auth", () => !isAuthenticated(api.Client), () => authenticate(api.Client))
        from _ in api.Methods(methods)
        from invariants in Combine.Checkrs(invariants.Select(a => a(api)))
        select Case.Closed;
}
