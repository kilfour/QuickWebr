using QuickCheckr;

namespace QuickWebr.Bolts.UpdateBuilders;

public record PoolCondition<TPoolElement>(
    Func<IReadOnlyCollection<TPoolElement>, bool>? OnCollection,
    Func<TPoolElement, bool>? OnElement
)
{
    public CheckrOf<(Func<bool>, CheckrOf<Case>)> GetCheckr(
        string route,
        Func<PoolElement<TPoolElement>, CheckrOf<Case>> checkr)
    {
        if (OnCollection != null)
            return Trackr.PoolWhen(route, OnCollection, checkr);
        if (OnElement != null)
            return Trackr.PoolWhen(route, OnElement, checkr);
        throw new InvalidOperationException();
    }
}
