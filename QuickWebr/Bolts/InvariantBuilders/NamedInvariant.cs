using QuickCheckr;

namespace QuickWebr.Bolts.InvariantBuilders;

public class NamedInvariant<TReader>(string name)
{
    public Observation<TReader> ForAll<TPoolElement>(Func<TReader, TPoolElement, bool> expectation) =>
        new(a => Trackr.PoolExpectEach<TPoolElement>(name, b => expectation(a, b)));
}