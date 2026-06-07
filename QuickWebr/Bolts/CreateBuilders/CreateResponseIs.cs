using QuickFuzzr;

namespace QuickWebr.Bolts.CreateBuilders;

public class CreateResponseIs<TReader, TPoolElement, TRequest>(
    string name,
    string route,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    FuzzrOf<TRequest> fuzzr)
{
    public CreateStore<TReader, TPoolElement, TRequest, TResponse> ResponseIs<TResponse>(Func<TResponse, bool> responseCheck)
        => new(name, route, predicate, fuzzr, responseCheck);
    public CreateStore<TReader, TPoolElement, TRequest, TResponse> ResponseIs<TResponse>()
        => new(name, route, predicate, fuzzr, a => true);
}
