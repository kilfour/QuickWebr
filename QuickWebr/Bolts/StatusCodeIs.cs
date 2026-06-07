using System.Net;
using QuickCheckr;
using QuickFuzzr;

namespace QuickWebr.Bolts;

public static class StatusCodeIs
{
    public static CheckrOf<Case> Success(string route, ActResult<HttpResponseMessage> response) =>
        from expect in Checkr.Expect($"'{route}' Status Code is Success", () => Success(response.Result), () => response.Result.StatusCode.ToString())
        // from trace in Checkr.TraceWhen("Bad Request",
        //     () => !Success(response), () => GetProblem(response)!)
        select Case.Closed;

    public static CheckrOf<Case> Success(string route, HttpResponseMessage response) =>
        from expect in Checkr.Expect($"'{route}' Status Code is Success", () => Success(response), response.StatusCode.ToString)
        from trace in Checkr.TraceWhen("Bad Request",
            () => !Success(response), () => GetProblem(response)!)
        select Case.Closed;

    private static bool Success(HttpResponseMessage response) =>
         response.StatusCode == HttpStatusCode.OK ||
         response.StatusCode == HttpStatusCode.NoContent ||
         response.StatusCode == HttpStatusCode.Created;

    private static string? GetProblem(HttpResponseMessage response)
    {
        // if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
        //     return null;
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        // var problem = response.Content.ReadFromJsonAsync<ProblemDetails>().GetAwaiter().GetResult();
        // return problem!.Detail;
    }
}