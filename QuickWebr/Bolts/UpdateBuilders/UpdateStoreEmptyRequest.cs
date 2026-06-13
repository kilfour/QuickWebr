namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateStoreEmptyRequest<TReader, TPoolElement, TRouteId>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public UpdateReadBackEmptyRequest<TReader, TPoolElement, TRouteId> Store(
        Func<TPoolElement, TPoolElement> update)
            => new(name, httpMethod, poolCondition, getRouteId, routeFactory, update);
}