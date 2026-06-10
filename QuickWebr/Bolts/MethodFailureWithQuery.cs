using System.Net;
using System.Net.Http.Json;
using QuickCheckr;
using QuickFuzzr;
using QuickPulse.Bolts;

namespace QuickWebr.Bolts;

public class MethodFailureWithQuery<TPoolElement>(
    HttpStatusCode statusCode,
    string label,
    string route,
    Func<TPoolElement, (string, string)> mutate)
{
    public CheckrOf<Case> GetCheckr(HttpClient client, TPoolElement element) =>
        from flag in Trackr.Stashed(label, () => new Cell<bool>(true))
        from maybe in Checkr.When(() => flag.Value,
            from query in Checkr.Capture(() => mutate(element))
            from actualRoute in Checkr.Capture(() => $"{route}?{query.Item1}={query.Item2}")
            from traceRout in Checkr.Trace($"{label} Route", () => actualRoute)
            from response in Checkr.ShrinkableAct(label,
                () => Send<TPoolElement>.Request(
                    client,
                    HttpMethod.Get,
                    actualRoute))
            from checks in Checkr.Expect($"{label}, Status Code", () => response.Result.StatusCode == statusCode,
                () => [$"   Route: {route}", $"Expected: {statusCode}", $"  Actual: {response.Result.StatusCode}"])
            from flip in Checkr.Perform(() => flag.Value = !flag.Value)
            select Case.Closed)
        select Case.Closed;
}


public class Alternate<TPoolElement, TResponse>(
    string label,
    string route,
    Func<TPoolElement, (string, string)> mutate,
    Func<TResponse, TPoolElement, bool> expectation)
{
    public CheckrOf<Case> GetCheckr(HttpClient client, TPoolElement element) =>
        from flag in Trackr.Stashed(label, () => new Cell<bool>(true))
        from maybe in Checkr.When(() => flag.Value,
            from query in Checkr.Capture(() => mutate(element))
            from actualRoute in Checkr.Capture(() => $"{route}?{query.Item1}={query.Item2}")
            from traceRout in Checkr.Trace($"{label} Route", () => actualRoute)
            from response in Checkr.ShrinkableAct(label,
                () => Send<TPoolElement>.Request(
                    client,
                    HttpMethod.Get,
                    actualRoute))
            from result in Checkr.Capture(() => response.Result.Content.ReadFromJsonAsync<TResponse>())
            from checks in Checkr.Expect($"{label}, Status Code", () => expectation(result, element),
                () => $"   Route: {route}")
            from flip in Checkr.Perform(() => flag.Value = !flag.Value)
            select Case.Closed)
        select Case.Closed;
}