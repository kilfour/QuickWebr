using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders.MultiplePools;

public class UpdateSend<TReader, TPoolElementOne, TPoolElementTwo, TRouteId>(
    string name,
    HttpMethod httpMethod,
    Func<TPoolElementOne, TPoolElementTwo, bool> predicate,
    Func<TPoolElementOne, TPoolElementTwo, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public UpdateStore<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId> Send<TRequest>(
        Func<TPoolElementOne, TPoolElementTwo, FuzzrOf<TRequest>> fuzzrFactory)
            => new(name, httpMethod, predicate, fuzzrFactory, getRouteId, routeFactory);

    public UpdateStore<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId> Send<TRequest>(
        Func<TPoolElementOne, TPoolElementTwo, TRequest> requestFactory)
        => new(name, httpMethod, predicate, (a, b) => Fuzzr.Constant(requestFactory(a, b)), getRouteId, routeFactory);
}
