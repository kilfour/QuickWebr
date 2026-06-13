namespace QuickWebr.Bolts.UpdateBuilders.MultiplePools;

public class UpdateRoute<TReader, TPoolElementOne, TPoolElementTwo>(
    string name,
    HttpMethod httpMethod,
    Func<TReader, TPoolElementOne, TPoolElementTwo, bool> predicate)
{
    public UpdateSend<TReader, TPoolElementOne, TPoolElementTwo, TRouteId> Route<TRouteId>(
        Func<TPoolElementOne, TPoolElementTwo, TRouteId> getRouteId,
        Func<TRouteId, string> routeFactory)
            => new(name, httpMethod, predicate, getRouteId, routeFactory);
}
