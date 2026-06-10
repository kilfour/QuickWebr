using QuickFuzzr;

namespace QuickWebr.Bolts.CreateBuilders;

public class CreateReadBack<TReader, TRequest, TResponse, TPoolElement>(
    string name,
    string route,
    FuzzrOf<TRequest> fuzzr,
    Func<TResponse, bool> responseCheck,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    Func<TRequest, TResponse, TPoolElement> toPool)
{
    public CreateExpect<TReader, TRequest, TResponse, TPoolElement, TDbValue> ReadBack<TDbValue>(
        Func<TReader, TPoolElement, TDbValue> read) =>
        new(name, route, fuzzr, responseCheck, predicate, toPool, read);
}
