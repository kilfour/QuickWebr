using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using QuickCheckr;
using QuickFuzzr;

namespace QuickWebr.Bolts;

public static class StatusCodeIs
{
    public static CheckrOf<Case> Success(string route, ActResult<HttpResponseMessage> response) =>
        from expect in Checkr.Expect($"'{route}' Status Code is Success",
        () => Success(response.Result),
        () => [.. GetProblem(response.Result).Prepend(response.Result.StatusCode.ToString())])
        // response.Result.StatusCode.ToString()
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

    private static List<string> GetProblem(HttpResponseMessage response)
    {
        // var problem = response.ReadValidationProblemAsync().GetAwaiter().GetResult();
        // if (problem is not null)
        // {
        //     return [.. problem.Errors.SelectMany(kv =>
        //         kv.Value.Select(error => $"{kv.Key}: {error}"))];
        // }
        // if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
        //     return null;
        return [response.Content.ReadAsStringAsync().GetAwaiter().GetResult()];
        // var problem = response.Content.ReadFromJsonAsync<ProblemDetails>().GetAwaiter().GetResult();
        // return problem!.Detail;
    }
}

public static class HttpResponseMessageExtensions
{
    public static async Task<ValidationProblemDetails> ReadValidationProblemAsync(
        this HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        return problem ?? throw new InvalidOperationException(
            "Response did not contain a valid ValidationProblemDetails body.");
    }
}