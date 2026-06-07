namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateRoute<TReader, TPoolElement>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition)
{
    public UpdateSend<TReader, TPoolElement, TRouteId> Route<TRouteId>(
        Func<TPoolElement, TRouteId> getRouteId,
        Func<TRouteId, string> routeFactory)
            => new(name, httpMethod, poolCondition, getRouteId, routeFactory);
}

