namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateReadBackEmptyRequest<TReader, TPoolElement, TRouteId>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TPoolElement> store)
{
    public UpdateExpectEmptyRequest<TReader, TPoolElement, TRouteId, TDbValue> ReadBack<TDbValue>(
        Func<TReader, TPoolElement, TDbValue> read)
            => new(name, httpMethod, poolCondition, getRouteId, routeFactory, store, read);
}

