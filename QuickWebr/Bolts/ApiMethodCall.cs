using System.Net;
using System.Net.Http.Json;
using QuickCheckr;
using QuickFuzzr;

namespace QuickWebr.Bolts;

public class ApiMethodCall(string route, CheckrOf<HttpResponseMessage> requestMethod)
{
    public CheckrOf<TResponse> Returns<TResponse>() =>
        from response in requestMethod
        from responseIsSuccess in StatusCodeIsSuccess(route, response)
        from result in Checkr.Capture(response.Content.ReadFromJsonAsync<TResponse>())
        select result;

    public CheckrOf<Case> ReturnsNothing() =>
        from response in requestMethod
        from responseIsSuccess in StatusCodeIsSuccess(route, response)
        select Case.Closed;

    private static CheckrOf<Case> StatusCodeIsSuccess(string route, HttpResponseMessage response) =>
        from expect in Checkr.Expect($"'{route}' Status Code is Success", () => StatusCodeIsSuccess(response))
        from trace in Checkr.TraceWhen("Bad Request",
            () => !StatusCodeIsSuccess(response), () => GetProblem(response)!)
        select Case.Closed;

    private static bool StatusCodeIsSuccess(HttpResponseMessage response) =>
         response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;

    private static string? GetProblem(HttpResponseMessage response)
    {
        // if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
        //     return null;
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        // var problem = response.Content.ReadFromJsonAsync<ProblemDetails>().GetAwaiter().GetResult();
        // return problem!.Detail;
    }
}