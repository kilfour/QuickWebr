namespace QuickWebr.Bolts.GetBuilders;

public class GetResponseIs<TReader, TPoolElement>(
    string name,
    string route)
{
    public GetExpect<TReader, TPoolElement, TResponse> ResponseIs<TResponse>(Func<TResponse, bool> responseCheck)
        => new(name, route, responseCheck);
    public GetExpect<TReader, TPoolElement, TResponse> ResponseIs<TResponse>()
        => new(name, route, a => true);
}
