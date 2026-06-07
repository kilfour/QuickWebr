using QuickCheckr;
using QuickFuzzr;

namespace QuickWebr.Bolts;

public record RequestInfo<TPoolElement, TRequest>(
    FuzzrOf<TRequest> Fuzzr,
    Shrinker[] Shrinkers,
    Func<TPoolElement, Shrinker>? ShrinkerFactory)
{
    public Shrinker[] GetShrinkers(TPoolElement element) =>
        ShrinkerFactory == null ? Shrinkers : [.. Shrinkers, ShrinkerFactory(element)];
};
