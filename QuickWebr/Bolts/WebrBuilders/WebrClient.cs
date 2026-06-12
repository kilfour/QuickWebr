using QuickWebr.Bolts.WebrBuilders;

namespace QuickWebr;

public class WebrClient<T>(string name, Func<T> contextFactory, Func<T, HttpClient> clientFactory)
{
    private Func<HttpClient, bool> isAuthenticated = a => true;
    private Func<HttpClient, Task> authenticate = a => Task.CompletedTask;

    public WebrClient<T> Authentication(Func<HttpClient, bool> isAuthenticated, Func<HttpClient, Task> authenticate)
    {
        this.isAuthenticated = isAuthenticated;
        this.authenticate = authenticate;
        return this;
    }

    public WebrRunner<T, TReader> Reader<TReader>(Func<T, TReader> readerFactory) =>
        WebrRunner<T, TReader>.Named(name, contextFactory, clientFactory, isAuthenticated, authenticate, readerFactory);
}