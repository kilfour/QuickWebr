namespace QuickWebr.Bolts.DeleteBuilders;

public class DeleteReadBack<TReader, TPoolElement, TRouteId>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public DeleteExpect<TReader, TPoolElement, TRouteId, TDbValue> ReadBack<TDbValue>(
        Func<TReader, TPoolElement, TDbValue> read)
            => new(name, httpMethod, poolCondition, getRouteId, routeFactory, read);
}
