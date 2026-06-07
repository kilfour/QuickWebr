namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateReadBack<TReader, TPoolElement, TRequest, TRouteId>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TRequest, TPoolElement> store)
{
    public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> ReadBack<TDbValue>(
        Func<TReader, TPoolElement, TDbValue> read)
            => new(name, httpMethod, poolCondition, requestInfo, getRouteId, routeFactory, store, read);
}

