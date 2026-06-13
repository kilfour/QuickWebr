using QuickWebr.Bolts.WebrBuilders;

namespace QuickWebr;

public class WebrContext<T>(string name, Func<T> contextFactory)
{
    public WebrClient<T> Client(Func<T, HttpClient> clientFactory) => new(name, contextFactory, clientFactory);
}
