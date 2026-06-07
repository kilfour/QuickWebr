namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateStore<TReader, TPoolElement, TRequest, TRouteId>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public UpdateReadBack<TReader, TPoolElement, TRequest, TRouteId> Store(
        Func<TPoolElement, TRequest, TPoolElement> update)
            => new(name, httpMethod, poolCondition, requestInfo, getRouteId, routeFactory, update);
}

