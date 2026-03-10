using Microsoft.EntityFrameworkCore;
using QuickCheckr;
using QuickCheckr.UnderTheHood;

namespace QuickWebr.Bolts.UpdateBuilders;


public class UpdateNoBodyBuilder<TPoolElement, TRouteId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory
)
{
    public UpdateNoBodyBuilderStored<TPoolElement, TRouteId> Store(Func<TPoolElement, TPoolElement> update)
        => new(api, route, poolCondition, getRouteId, routeFactory, update);
}

public class UpdateNoBodyBuilderStored<TPoolElement, TRouteId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TPoolElement> update
)
{
    public UpdateNoBodyBuilderStored<TPoolElement, TRouteId, TDbValue> Load<TDbValue>(Func<DbContext, TRouteId, TDbValue> loader)
        => new(api, route, poolCondition, getRouteId, routeFactory, update, loader);
}


public class UpdateNoBodyBuilderStored<TPoolElement, TRouteId, TDbValue>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TPoolElement> update,
    Func<DbContext, TRouteId, TDbValue> loader
)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TDbValue, bool> expectation)[] expectations) =>
        poolCondition.GetCheckr(route, element =>
            from routeId in Checkr.Capture(() => getRouteId(element.Value))
            from response in api.LabeledRoute(route, routeFactory(routeId)).ReturnsNothing()
            from reloaded in api.GetEntityCheckr((db) => loader(db, routeId))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(reloaded))))
            from store in element.Replace(update(element.Value))
            select Case.Closed);
}
