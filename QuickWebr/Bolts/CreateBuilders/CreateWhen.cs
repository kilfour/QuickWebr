namespace QuickWebr.Bolts.CreateBuilders;

public class CreateWhen<TReader>(string name)
{
    public CreateRoute<TReader, TPoolElement> When<TPoolElement>(
        Func<IReadOnlyCollection<TPoolElement>, bool> predicate)
        => new(name, predicate);

    public CreateRoute<TReader, TPoolElement> Always<TPoolElement>()
        => When<TPoolElement>(_ => true);
}
