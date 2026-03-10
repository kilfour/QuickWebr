using Microsoft.EntityFrameworkCore;
using QuickCheckr;
using QuickCheckr.UnderTheHood;

namespace QuickWebr.Bolts.UpdateBuilders;


public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2>(
    Spider api,
    string route,
    Func<TPoolElement1, TPoolElement2, bool> predicate)
{
    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest> With<TRequest>(
        Func<TPoolElement1, TPoolElement2, TRequest> request)
        => new(api, route, predicate, request);
}

public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest>(
    Spider api,
    string route,
    Func<TPoolElement1, TPoolElement2, bool> predicate,
    Func<TPoolElement1, TPoolElement2, TRequest> request)
{
    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId> Route<TRouteId>(
        Func<TPoolElement1, TPoolElement2, TRouteId> getRouteId,
        Func<TRouteId, string> routeFactory)
        => new(api, route, predicate, request, getRouteId, routeFactory);
}

public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId>(
    Spider api,
    string route,
    Func<TPoolElement1, TPoolElement2, bool> predicate,
    Func<TPoolElement1, TPoolElement2, TRequest> request,
    Func<TPoolElement1, TPoolElement2, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId, TDbValue1, TDbValue2> Load<TDbValue1, TDbValue2>(
            Func<DbContext, TPoolElement1, TPoolElement2, (TDbValue1, TDbValue2)> loader)
        => new(api, route, predicate, request, getRouteId, routeFactory, loader);

}

public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId, TDbValue1, TDbValue2>(
    Spider api,
    string route,
    Func<TPoolElement1, TPoolElement2, bool> predicate,
    Func<TPoolElement1, TPoolElement2, TRequest> request,
    Func<TPoolElement1, TPoolElement2, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<DbContext, TPoolElement1, TPoolElement2, (TDbValue1, TDbValue2)> loader)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TDbValue1, TDbValue2, bool> expectation)[] expectations) =>
        Trackr.PoolWhen($"{route} from Pool", predicate!, (element1, element2) =>
            from routeId in Checkr.Capture(() => getRouteId(element1.Value, element2.Value))
            from response in api.LabeledRoute(route, routeFactory(routeId), request(element1.Value, element2.Value)).ReturnsNothing()
            from reloaded in api.GetEntityCheckr((db) => loader(db, element1.Value, element2.Value))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(reloaded.Item1, reloaded.Item2))))
            select Case.Closed);
}