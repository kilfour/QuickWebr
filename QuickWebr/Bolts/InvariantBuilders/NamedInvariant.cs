using QuickCheckr;
using QuickCheckr.Diagnostics;

namespace QuickWebr.Bolts.InvariantBuilders;

public class NamedInvariant<TReader>(string name)
{
    public Observation<TReader> ForAll<TPoolElement>(Func<TReader, TPoolElement, bool> expectation) =>
        new(a =>
            from itr in Trackr.PoolForEach<TPoolElement>(el => Autopsy.Note(el.Id.ToString(), () => expectation(a, el.Value)))
            from invariant in Trackr.PoolExpectEach<TPoolElement>(name, b => expectation(a, b))
            select Case.Closed);
}