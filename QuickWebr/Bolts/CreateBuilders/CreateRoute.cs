namespace QuickWebr.Bolts.CreateBuilders;

public class CreateRoute<TReader, TPoolElement>(
    string name,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
{
    public CreateSend<TReader, TPoolElement> Route(string route)
        => new(name, route, predicate);
}
