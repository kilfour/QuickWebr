using System.Net;
using QuickCheckr;
using QuickFuzzr;
using QuickPulse.Bolts;

namespace QuickWebr.Bolts;


public class MethodNoIdFailure<TPoolElement, TRequest>(
    HttpStatusCode statusCode,
    string label,
    string route,
    Func<TRequest, TRequest> mutate)
{
    public CheckrOf<Case> GetCheckr(HttpClient client, HttpMethod httpMethod, TRequest request) =>
        from flag in Trackr.Stashed(label, () => new Cell<bool>(true))
        from maybe in Checkr.When(() => flag.Value,
            from mutated in Checkr.Capture(() => mutate(request))
            from response in Checkr.ShrinkableAct(label, () =>
                Send<TRequest>.Request(client, httpMethod, route, mutated))
            from checks in Checkr.Expect($"{label}, Status Code", () => response.Result.StatusCode == statusCode)
            from flip in Checkr.Perform(() => flag.Value = !flag.Value)
            select Case.Closed)
        select Case.Closed;
}

public class MethodFailure<TPoolElement, TRequest, TRouteId>(
    HttpStatusCode statusCode,
    string label,
    Func<TPoolElement, TRequest, (TPoolElement, TRequest)> mutate,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public CheckrOf<Case> GetCheckr(HttpClient client, HttpMethod httpMethod, TPoolElement element, TRequest request) =>
        from flag in Trackr.Stashed(label, () => new Cell<bool>(true))
        from maybe in Checkr.When(() => flag.Value,
            from tuple in Checkr.Capture(() => mutate(element, request))
            from response in Checkr.ShrinkableAct(label, () =>
                Send<TRequest>.Request(
                        client,
                        httpMethod,
                        routeFactory(getRouteId(tuple.Item1)),
                        tuple.Item2))
            from checks in Checkr.Expect($"{label}, Status Code", () => response.Result.StatusCode == statusCode)
            from flip in Checkr.Perform(() => flag.Value = !flag.Value)
            select Case.Closed)
        select Case.Closed;
}

public class MethodFailure<TPoolElement, TRouteId>(
    HttpStatusCode statusCode,
    string label,
    Func<TPoolElement, TPoolElement> mutate,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory)
{
    public CheckrOf<Case> GetCheckr(HttpClient client, HttpMethod httpMethod, TPoolElement element) =>
        from flag in Trackr.Stashed(label, () => new Cell<bool>(true))
        from maybe in Checkr.When(() => flag.Value,
            from route in Checkr.Capture(() => routeFactory(getRouteId(mutate(element))))
            from response in Checkr.ShrinkableAct(label, () =>
                Send.Request(client, httpMethod, route))
            from checks in Checkr.Expect($"{label}, Status Code", () => response.Result.StatusCode == statusCode,
                () => [$"   Route: {route}", $"Expected: {statusCode}", $"  Actual: {response.Result.StatusCode}"])
            from flip in Checkr.Perform(() => flag.Value = !flag.Value)
            select Case.Closed)
        select Case.Closed;
}
