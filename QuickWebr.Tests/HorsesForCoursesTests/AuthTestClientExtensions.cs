using System.Net.Http.Headers;
using System.Net.Http.Json;
using HorsesForCourses.Api.Accounts;

namespace QuickWebr.Tests.HorsesForCoursesTests;

public static class AuthTestClientExtensions
{
    public static async Task AuthenticateViaTokenEndpointAsync(this HttpClient client, string email = "a@b.com", string password = "pw")
    {
        var res = await client.PostAsJsonAsync("/auth/token", new LoginRequest(email, password));
        res.EnsureSuccessStatusCode();

        var payload = await res.Content.ReadFromJsonAsync<TokenResponse>()
                      ?? throw new InvalidOperationException("Missing token response.");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(payload.token_type, payload.access_token);
    }

    // needs to match controller's JSON property names
    public sealed record TokenResponse(string access_token, string token_type, int expires_in);
}

