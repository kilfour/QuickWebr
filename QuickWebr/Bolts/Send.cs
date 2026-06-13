using System.Net.Http.Json;

namespace QuickWebr.Bolts;

public static class Send<TRequest>
{
    public static Task<HttpResponseMessage> Request(
        HttpClient client,
        HttpMethod httpMethod,
        string route,
        TRequest request)
            => httpMethod.Method switch
            {

                "POST" => client.PostAsJsonAsync(route, request),
                "PUT" => client.PutAsJsonAsync(route, request),
                _ => throw new NotSupportedException()
            };
}


public static class Send
{
    public static Task<HttpResponseMessage> Request(
        HttpClient client,
        HttpMethod httpMethod,
        string route)
            => httpMethod.Method switch
            {
                "GET" => client.GetAsync(route),
                "POST" => client.PostAsync(route, null),
                "PUT" => client.PutAsync(route, null),
                "DELETE" => client.DeleteAsync(route),
                _ => throw new NotSupportedException()
            };
}