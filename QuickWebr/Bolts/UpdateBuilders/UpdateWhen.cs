using QuickWebr.Bolts.UpdateBuilders.MultiplePools;

namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateWhen<TReader>(string name, HttpMethod httpMethod)
{
    public UpdateRoute<TReader, TPoolElement> When<TPoolElement>(Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(name, httpMethod, new PoolCondition<TPoolElement>(predicate, null));

    public UpdateRoute<TReader, TPoolElement> When<TPoolElement>(Func<TPoolElement, bool> predicate)
        => new(name, httpMethod, new PoolCondition<TPoolElement>(null, predicate));

    public UpdateRoute<TReader, TPoolElementOne, TPoolElementTwo> When<TPoolElementOne, TPoolElementTwo>(
        Func<TPoolElementOne, TPoolElementTwo, bool> predicate)
            => new(name, httpMethod, predicate);
}

