using QuickFuzzr;

namespace QuickWebr.Bolts.GetBuilders;

public class GetSendQuery<TReader, TPoolElement>(
    string name,
    Func<IReadOnlyCollection<TPoolElement>, bool> poolCondition,
    string route)
{
    public GetResponseIs<TReader, TPoolElement> Send()
        => new(name, poolCondition, route, null);

    public GetResponseIs<TReader, TPoolElement> SendQuery(Func<TPoolElement, (string, FuzzrOf<string>)> queryValue)
        => new(name, poolCondition, route, queryValue);
}
