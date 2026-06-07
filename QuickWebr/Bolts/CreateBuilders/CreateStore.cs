using QuickFuzzr;

namespace QuickWebr.Bolts.CreateBuilders;

public class CreateStore<TReader, TPoolElement, TRequest, TResponse>(
    string name,
    string route,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    FuzzrOf<TRequest> fuzzr,
    Func<TResponse, bool> responseCheck)
{
    public CreateReadBack<TReader, TRequest, TResponse, TPoolElement> Store(Func<TResponse, TPoolElement> toPool) =>
        new(name, route, fuzzr, responseCheck, predicate, (req, res) => toPool(res));

    public CreateReadBack<TReader, TRequest, TResponse, TPoolElement> Store(Func<TRequest, TResponse, TPoolElement> toPool) =>
        new(name, route, fuzzr, responseCheck, predicate, toPool);
}
