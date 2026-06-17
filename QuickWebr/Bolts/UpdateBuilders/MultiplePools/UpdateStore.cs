namespace QuickWebr.Bolts.UpdateBuilders.MultiplePools;

public class UpdateStore<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId>(
    string name,
    HttpMethod httpMethod,
    Func<TReader, TPoolElementOne, TPoolElementTwo, bool> predicate,
    Func<TPoolElementOne, TPoolElementTwo, TRequest> requestFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public UpdateReadBack<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId> Store(
        Func<TPoolElementOne, TPoolElementTwo, TRequest, TPoolElementOne> update)
            => new(name, httpMethod, predicate, requestFactory, getRouteId, routeFactory, update);
}
