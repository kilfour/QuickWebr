using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders.MultiplePools;

public class UpdateReadBack<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId>(
    string name,
    HttpMethod httpMethod,
    Func<TPoolElementOne, TPoolElementTwo, bool> predicate,
    Func<TPoolElementOne, TPoolElementTwo, FuzzrOf<TRequest>> fuzzrFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRequest, TPoolElementOne> store)
{
    public UpdateExpect<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId, TDbValueOne, TDbValueTwo> ReadBack<TDbValueOne, TDbValueTwo>(
        Func<TReader, TPoolElementOne, TPoolElementTwo, (TDbValueOne, TDbValueTwo)> read)
            => new(name, httpMethod, predicate, fuzzrFactory, getRouteId, routeFactory, store, read);
}
