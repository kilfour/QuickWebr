namespace QuickWebr.Bolts.GetBuilders;

public class GetWhen<TReader>(string name)
{
    public GetRoute<TReader, TPoolElement> When<TPoolElement>(Func<IReadOnlyCollection<TPoolElement>, bool> poolCondition)
        => new(name, poolCondition);

    public GetRoute<TReader, TPoolElement> Always<TPoolElement>()
         => new(name, _ => true);
}
