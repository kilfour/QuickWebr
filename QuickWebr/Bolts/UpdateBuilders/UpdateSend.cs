using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateSend<TReader, TPoolElement, TRouteId>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public UpdateStore<TReader, TPoolElement, TRequest, TRouteId> Send<TRequest>(FuzzrOf<TRequest> fuzzr)
        => new(name, httpMethod, poolCondition, new RequestInfo<TPoolElement, TRequest>(fuzzr, [], null), getRouteId, routeFactory);

    public UpdateStoreEmptyRequest<TReader, TPoolElement, TRouteId> Send()
            => new(name, httpMethod, poolCondition, getRouteId, routeFactory);
}

