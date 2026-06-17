namespace QuickWebr.Bolts.UpdateBuilders.MultiplePools;

public class UpdateReadBack<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId>(
    string name,
    HttpMethod httpMethod,
    Func<TReader, TPoolElementOne, TPoolElementTwo, bool> predicate,
    Func<TPoolElementOne, TPoolElementTwo, TRequest> requestFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRequest, TPoolElementOne> store)
{
    public UpdateExpect<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId, TDbValueOne, TDbValueTwo> ReadBack<TDbValueOne, TDbValueTwo>(
        Func<TReader, TPoolElementOne, TPoolElementTwo, (TDbValueOne, TDbValueTwo)> read)
            => new(name, httpMethod, predicate, requestFactory, getRouteId, routeFactory, store, read);
}
