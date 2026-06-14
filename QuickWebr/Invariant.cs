using QuickWebr.Bolts.InvariantBuilders;

namespace QuickWebr;

public abstract class Invariant<TReader>
{
    protected static NamedInvariant<TReader> Named(string name) => new(name);

    public abstract Observation<TReader> Define();
}
