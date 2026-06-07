namespace QuickWebr.Bolts.DeleteBuilders;

public class DeleteRoute<TReader, TPoolElement>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition)
{
    public DeleteReadBack<TReader, TPoolElement, TRouteId> Route<TRouteId>(
        Func<TPoolElement, TRouteId> getRouteId,
        Func<TRouteId, string> routeFactory)
            => new(name, httpMethod, poolCondition, getRouteId, routeFactory);
}