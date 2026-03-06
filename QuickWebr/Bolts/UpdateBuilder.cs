using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts;

public record PoolCondition<TPoolElement>(
    Func<IReadOnlyCollection<TPoolElement>, bool>? OnCollection,
    Func<TPoolElement, bool>? OnElement
)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> GetCheckr(
        string route,
        Func<PoolElement<TPoolElement>, CheckrOf<Case>> checkr)
    {
        if (OnCollection != null)
            return Trackr.PoolWhen(route, OnCollection, checkr);
        if (OnElement != null)
            return Trackr.PoolWhen(route, OnElement, checkr);
        throw new InvalidOperationException();
    }
}

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
        => new(api, route, poolCondition, fuzzr);
}

public class UpdateBuilder<TPoolElement, TRequest>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    FuzzrOf<TRequest> fuzzr)
{
    public UpdateBuilder<TRequest, TPoolElement, TRouteId> RouteIdFrom<TRouteId>(Func<TPoolElement, TRouteId> getRouteId)
        => new(api, route, poolCondition, fuzzr, getRouteId);
}

public class UpdateBuilder<TRequest, TPoolElement, TRouteId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    FuzzrOf<TRequest> fuzzr,
    Func<TPoolElement, TRouteId> getRouteId)
{
    public UpdateBuilder<TRequest, TPoolElement, TRouteId, TEntityId> EntityIdFrom<TEntityId>(Func<TPoolElement, TEntityId> getEntityId)
        => new(api, route, poolCondition, fuzzr, getRouteId, getEntityId);
}

public class UpdateBuilder<TRequest, TPoolElement, TRouteId, TEntityId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    FuzzrOf<TRequest> fuzzr,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TPoolElement, TEntityId> getEntityId)
{
    public UpdateBuilderStored<TRequest, TPoolElement, TRouteId, TEntityId> Store(Func<TPoolElement, TRequest, TPoolElement> update)
        => new(api, route, poolCondition, fuzzr, getRouteId, getEntityId, update);
}

public class UpdateBuilderStored<TRequest, TPoolElement, TRouteId, TEntityId>(
    Spider api,
    string route,
    PoolCondition<TPoolElement> poolCondition,
    FuzzrOf<TRequest> fuzzr,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TPoolElement, TEntityId> getEntityId,
    Func<TPoolElement, TRequest, TPoolElement> update)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> As<TEntity>(
        params (string label, Func<TRequest, TEntity, bool> expectation)[] expectations)
        where TEntity : class =>
        poolCondition.GetCheckr(route, element =>
            from request in Checkr.Input($"'{route}' Request", fuzzr)
            from response in api.LabeledRoute(route, route.Replace("{id}", getRouteId(element.Value)!.ToString()), request).ReturnsNothing()
            from reloaded in api.GetByIdCheckr<TEntityId, TEntity>(getEntityId(element.Value))
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(request, reloaded))))
            from store in element.Replace(update(element.Value, request))
            select Case.Closed);
}

// ---------------------------------------------------------------------------------------------------------
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
    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId> RouteIdFrom<TRouteId>(
        Func<TPoolElement1, TPoolElement2, TRouteId> getRouteId)
        => new(api, route, predicate, request, getRouteId);
}

public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId>(
    Spider api,
    string route,
    Func<TPoolElement1, TPoolElement2, bool> predicate,
    Func<TPoolElement1, TPoolElement2, TRequest> request,
    Func<TPoolElement1, TPoolElement2, TRouteId> getRouteId)
{
    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId, TEntityId1, TEntityId2> EntityIdsFrom<TEntityId1, TEntityId2>(
            Func<TPoolElement1, TPoolElement2, (TEntityId1, TEntityId2)> getEntityIds)
        => new(api, route, predicate, request, getRouteId, getEntityIds);
}

public class CombinedUpdateBuilder<TPoolElement1, TPoolElement2, TRequest, TRouteId, TEntityId1, TEntityId2>(
    Spider api,
    string route,
    Func<TPoolElement1, TPoolElement2, bool> predicate,
    Func<TPoolElement1, TPoolElement2, TRequest> request,
    Func<TPoolElement1, TPoolElement2, TRouteId> getRouteId,
    Func<TPoolElement1, TPoolElement2, (TEntityId1, TEntityId2)> getEntityIds)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> As<TEntity1, TEntity2>(params (string label, Func<TEntity1, TEntity2, bool> expectation)[] expectations)
        where TEntity1 : class
        where TEntity2 : class
        => Trackr.PoolWhen($"{route} from Pool", predicate!, (element1, element2) =>
            from response in api.LabeledRoute(route, route.Replace("{id}", getRouteId(element1.Value, element2.Value)?.ToString()), request(element1.Value, element2.Value)).ReturnsNothing()
            from ids in Checkr.Capture(() => getEntityIds(element1.Value, element2.Value))
            from reloaded1 in api.GetByIdCheckr<TEntityId1, TEntity1>(ids.Item1)
            from reloaded2 in api.GetByIdCheckr<TEntityId2, TEntity2>(ids.Item2)
            from checks in Combine.Checkrs(expectations.Select(a =>
                Checkr.Expect($"'{route}' {a.label}", () => a.expectation(reloaded1, reloaded2))))
            select Case.Closed);

    // private CheckrOf<Case> MakeTheCall<TEntity1, TEntity2>(
    //     PoolElement<TPoolElement1> element1,
    //     PoolElement<TPoolElement2> element2,
    //     (string label, Func<TEntity1, TEntity2, bool> expectation)[] expectations)
    //     where TEntity1 : DomainEntity<TEntity1>
    //     where TEntity2 : DomainEntity<TEntity2> =>
    //         from response in api.Route(route, route.Replace("{id}", getId1(element1.Value).ToString()), request(element1.Value, element2.Value)).ReturnsNothing()
    //         from reloaded1 in api.GetByIdCheckr<TEntity1>(getId1(element1.Value))
    //         from reloaded2 in api.GetByIdCheckr<TEntity2>(getId2(element2.Value))
    //         from checks in Combine.Checkrs(expectations.Select(a =>
    //             Checkr.Expect($"'{route}' {a.label}", () => a.expectation(reloaded1, reloaded2))))
    //         select Case.Closed;
}