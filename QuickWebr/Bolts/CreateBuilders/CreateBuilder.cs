using Microsoft.EntityFrameworkCore;
using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.CreateBuilders;

public class CreateBuilder(Spider api, string route)
{
    public CreateBuilder<TRequest> With<TRequest>(FuzzrOf<TRequest> fuzzr)
        => new(api, route, fuzzr);
}

public class CreateBuilder<TRequest>(Spider api, string route, FuzzrOf<TRequest> fuzzr)
{
    public CreateBuilder<TRequest, TResponse> Returns<TResponse>(Func<TResponse, bool> responseCheck)
        => new(api, route, fuzzr, responseCheck);
}

public class CreateBuilder<TRequest, TResponse>(Spider api, string route, FuzzrOf<TRequest> fuzzr, Func<TResponse, bool> responseCheck)
{
    public CreateBuilder<TRequest, TResponse, TPoolElement> When<TPoolElement>(
        Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(api, route, fuzzr, responseCheck, predicate);
}

public class CreateBuilder<TRequest, TResponse, TPoolElement>(
    Spider api,
    string route,
    FuzzrOf<TRequest> fuzzr,
    Func<TResponse, bool> responseCheck,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
{
    public CreateBuilderLoader<TRequest, TResponse, TPoolElement> Store(Func<TResponse, TPoolElement> toPool) =>
        new(api, route, fuzzr, responseCheck, predicate, toPool);
}

public class CreateBuilderLoader<TRequest, TResponse, TPoolElement>(
    Spider api,
    string route,
    FuzzrOf<TRequest> fuzzr,
    Func<TResponse, bool> responseCheck,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    Func<TResponse, TPoolElement> toPool)
{
    public CreateBuilderFinal<TRequest, TResponse, TPoolElement, TDbValue> Load<TDbValue>(Func<DbContext, TResponse, TDbValue> loader) =>
        new(api, route, fuzzr, responseCheck, predicate, toPool, loader);
}

public class CreateBuilderFinal<TRequest, TResponse, TPoolElement, TDbValue>(
    Spider api,
    string route,
    FuzzrOf<TRequest> fuzzr,
    Func<TResponse, bool> responseCheck,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    Func<TResponse, TPoolElement> toPool,
    Func<DbContext, TResponse, TDbValue> loader)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations) =>
        Trackr.PoolWhen(predicate,
            from request in Checkr.Input($"'{route}' Request", fuzzr)
            from response in api.Route(route, request).Returns<TResponse>()
            from created in Checkr.Expect($"'{route}' Response", () => responseCheck(response))
            from store in Trackr.ToPool($"'{route}' to Pool", toPool(response))
            from reloaded in api.GetEntityCheckr((db) => loader(db, response))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(request, reloaded))))
            select Case.Closed);
}