using QuickFuzzr;

namespace QuickWebr.Bolts.GetBuilders;

public class GetResponseIs<TReader, TPoolElement>(
    string name,
    Func<IReadOnlyCollection<TPoolElement>, bool> poolCondition,
    string route,
    Func<TPoolElement, (string, FuzzrOf<string>)>? queryValue)
{
    public GetExpect<TReader, TPoolElement, TResponse> ResponseIs<TResponse>(Func<TResponse, bool> responseCheck)
        => new(name, poolCondition, route, queryValue, responseCheck);
    public GetExpect<TReader, TPoolElement, TResponse> ResponseIs<TResponse>()
        => new(name, poolCondition, route, queryValue, a => true);
}
