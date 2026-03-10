using Microsoft.EntityFrameworkCore;
using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateBuilder(Spider api, string route)
{
    public UpdateBuilder<TPoolElement> When<TPoolElement>(Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(api, route, new PoolCondition<TPoolElement>(predicate, null));

    public UpdateBuilder<TPoolElement> When<TPoolElement>(Func<TPoolElement, bool> predicate)
        => new(api, route, new PoolCondition<TPoolElement>(null, predicate));

    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2> When<TPoolElement1, TPoolElement2>(
            Func<TPoolElement1, TPoolElement2, bool> predicate)
        => new(api, route, predicate);
}

public class UpdateBuilder<TPoolElement>(Spider api, string route, PoolCondition<TPoolElement> poolCondition)
{
    public UpdateBuilder<TPoolElement, TRequest> With<TRequest>(FuzzrOf<TRequest> fuzzr)
        => new(api, route, poolCondition, new RequestInfo<TPoolElement, TRequest>(fuzzr, [], null));

    public UpdateBuilder<TPoolElement, TRequest> With<TRequest>(FuzzrOf<TRequest> fuzzr, IClassificationOption classificationOption)
        => new(api, route, poolCondition, new RequestInfo<TPoolElement, TRequest>(fuzzr, [new Shrinker(() => [classificationOption], [])], null));

    public UpdateBuilder<TPoolElement, TRequest> With<TRequest>(FuzzrOf<TRequest> fuzzr, Func<TPoolElement, Shrinker> customShrinker)
        => new(api, route, poolCondition, new RequestInfo<TPoolElement, TRequest>(fuzzr, [], customShrinker));

    public UpdateNoBodyBuilder<TPoolElement, TRouteId> Route<TRouteId>(
        Func<TPoolElement, TRouteId> getRouteId, Func<TRouteId, string> routeFactory)
        => new(api, route, poolCondition, getRouteId, routeFactory);
}

public class UpdateBuilder<TPoolElement, TRequest>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo)
{
    public UpdateBuilder<TRequest, TPoolElement, TRouteId> Route<TRouteId>(
        Func<TPoolElement, TRouteId> getRouteId, Func<TRouteId, string> routeFactory)
        => new(api, route, poolCondition, requestInfo, getRouteId, routeFactory);
}

public class UpdateBuilder<TRequest, TPoolElement, TRouteId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public UpdateBuilderStored<TRequest, TPoolElement, TRouteId> Store(Func<TPoolElement, TRequest, TPoolElement> update)
        => new(api, route, poolCondition, requestInfo, getRouteId, routeFactory, update);

    public UpdateBuilder<TRequest, TPoolElement, TRouteId, TDbValue> Load<TDbValue>(Func<DbContext, TRouteId, TDbValue> loader)
        => new(api, route, poolCondition, requestInfo, getRouteId, routeFactory, loader);
}
public class UpdateBuilder<TRequest, TPoolElement, TRouteId, TDbValue>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<DbContext, TRouteId, TDbValue> loader)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations) =>
        poolCondition.GetCheckr(route, element =>
            from request in Checkr.Input($"'{route}' Request", requestInfo.Fuzzr, requestInfo.GetShrinkers(element.Value))
            from routeId in Checkr.Capture(() => getRouteId(element.Value))
            from response in api.LabeledRoute(route, routeFactory(routeId), request).ReturnsNothing()
            from reloaded in api.GetEntityCheckr((db) => loader(db, routeId))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(request, reloaded))))
            select Case.Closed);
}

public class UpdateBuilderStored<TRequest, TPoolElement, TRouteId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TRequest, TPoolElement> update)
{
    public UpdateBuilderStored<TRequest, TPoolElement, TRouteId, TDbValue> Load<TDbValue>(Func<DbContext, TRouteId, TDbValue> loader)
        => new(api, route, poolCondition, requestInfo, getRouteId, routeFactory, update, loader);
}

public class UpdateBuilderStored<TRequest, TPoolElement, TRouteId, TDbValue>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TRequest, TPoolElement> update,
    Func<DbContext, TRouteId, TDbValue> loader)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> Expect(
        params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations) =>
        poolCondition.GetCheckr(route, element =>
            from request in Checkr.Input($"'{route}' Request", requestInfo.Fuzzr, requestInfo.GetShrinkers(element.Value))
            from routeId in Checkr.Capture(() => getRouteId(element.Value))
            from response in api.LabeledRoute(route, routeFactory(routeId), request).ReturnsNothing()
            from reloaded in api.GetEntityCheckr((db) => loader(db, routeId))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(request, reloaded))))
            from store in element.Replace(update(element.Value, request))
            select Case.Closed);
}

