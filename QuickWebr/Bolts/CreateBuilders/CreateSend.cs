using QuickFuzzr;

namespace QuickWebr.Bolts.CreateBuilders;

public class CreateSend<TReader, TPoolElement>(
    string name,
    string route,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
{
    public CreateResponseIs<TReader, TPoolElement, TRequest> Send<TRequest>(FuzzrOf<TRequest> fuzzr)
        => new(name, route, predicate, fuzzr);
}
