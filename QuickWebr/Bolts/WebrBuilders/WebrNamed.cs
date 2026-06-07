namespace QuickWebr;

public class WebrNamed(string name)
{
    public WebrContext<T> Context<T>(Func<T> contextFactory) => new(name, contextFactory);
}
