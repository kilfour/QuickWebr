namespace QuickWebr.Bolts.GetBuilders;

public class GetRoute<TReader, TPoolElement>(
    string name,
    Func<IReadOnlyCollection<TPoolElement>, bool> poolCondition)
{
    public GetSendQuery<TReader, TPoolElement> Route(string route)
        => new(name, poolCondition, route);
}
