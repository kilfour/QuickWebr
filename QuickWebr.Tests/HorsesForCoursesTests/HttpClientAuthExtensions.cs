namespace QuickWebr.Tests.HorsesForCoursesTests;

public static class HttpClientAuthExtensions
{
    public static bool HasBearerToken(this HttpClient client) =>
        client.DefaultRequestHeaders.Authorization?.Scheme == "Bearer" &&
        !string.IsNullOrWhiteSpace(client.DefaultRequestHeaders.Authorization?.Parameter);
}