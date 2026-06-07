namespace QuickWebr.Bolts.GetBuilders;

public class GetWhen<TReader>(string name)
{
    // public GetRoute<TReader, TPoolElement> When<TPoolElement>(Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
    //     => new(name);

    // public GetRoute<TReader, TPoolElement> When<TPoolElement>(Func<TPoolElement, bool> predicate)
    //     => new(name);

    public GetRoute<TReader, TPoolElement> Always<TPoolElement>()
         => new(name);
}
