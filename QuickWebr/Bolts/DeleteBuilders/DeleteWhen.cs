namespace QuickWebr.Bolts.DeleteBuilders;

public class DeleteWhen<TReader>(string name, HttpMethod httpMethod)
{
    public DeleteRoute<TReader, TPoolElement> When<TPoolElement>(Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(name, httpMethod, new PoolCondition<TPoolElement>(predicate, null));

    public DeleteRoute<TReader, TPoolElement> When<TPoolElement>(Func<TPoolElement, bool> predicate)
        => new(name, httpMethod, new PoolCondition<TPoolElement>(null, predicate));
}
