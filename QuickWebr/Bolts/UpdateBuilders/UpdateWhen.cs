namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateWhen<TReader>(string name, HttpMethod httpMethod)
{
    public UpdateRoute<TReader, TPoolElement> When<TPoolElement>(Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(name, httpMethod, new PoolCondition<TPoolElement>(predicate, null));

    public UpdateRoute<TReader, TPoolElement> When<TPoolElement>(Func<TPoolElement, bool> predicate)
        => new(name, httpMethod, new PoolCondition<TPoolElement>(null, predicate));

}

