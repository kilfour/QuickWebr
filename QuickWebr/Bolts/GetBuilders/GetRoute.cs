namespace QuickWebr.Bolts.GetBuilders;

public class GetRoute<TReader, TPoolElement>(
    string name)
{
    public GetResponseIs<TReader, TPoolElement> Route(string route)
        => new(name, route);
}
