using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateExpectEmptyRequest<TReader, TPoolElement, TRouteId, TDbValue>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TPoolElement> store,
    Func<TReader, TPoolElement, TDbValue> read)
{
    // private readonly List<MethodFailure<TPoolElement, TRouteId>> failures = [];

    // public UpdateExpect<TReader, TPoolElement, TRouteId, TDbValue> FailsWith(
    //     string label,
    //     HttpStatusCode statusCode,
    //     Func<TPoolElement, (TPoolElement)> mutate)
    // {
    //     failures.Add(new MethodFailure<TPoolElement, TRouteId>(statusCode, $"'{name}' {label}", mutate, getRouteId, routeFactory));
    //     return this;
    // }

    // public UpdateExpect<TReader, TPoolElement, TRouteId, TDbValue> FailsWith(
    //     string label,
    //     HttpStatusCode statusCode,
    //     Func<TRequest, TRequest> mutate)
    // {
    //     failures.Add(new MethodFailure<TPoolElement, TRequest, TRouteId>(statusCode, $"'{name}' {label}", (e, r) => (e, mutate(r)), getRouteId, routeFactory));
    //     return this;
    // }

    private readonly List<(string TraceLabel, Func<TPoolElement, TDbValue, object> Factory)> traces = [];
    public UpdateExpectEmptyRequest<TReader, TPoolElement, TRouteId, TDbValue> Trace(
        string traceLabel,
        Func<TPoolElement, TDbValue, object> factory)
    {
        traces.Add((traceLabel, factory));
        return this;
    }

    public Specification<TReader> Expect(params (string label, Func<TDbValue, bool> expectation)[] expectations)
        => new(TheCheckr(expectations));

    private Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> TheCheckr(
        params (string label, Func<TDbValue, bool> expectation)[] expectations)
    {
        return (client, db) =>
            poolCondition.GetCheckr(name, element =>
                from route in Checkr.Capture(() => routeFactory(getRouteId(element.Value)))
                from traceRoute in Checkr.Trace("Route", () => route)
                from response in Checkr.ShrinkableAct(name,
                    () => Send.Request(client, httpMethod, route))
                from guard in Checkr.When(() => response.HasExecuted,
                    from responseIsSuccess in StatusCodeIs.Success(name, response)
                    from stored in element.Replace(store(element.Value))
                    from reloaded in Checkr.Capture(() => read(db, element.Value))
                    from traces in Combine.Checkrs(traces.Select(a =>
                        Checkr.Trace($"'{name}' {a.TraceLabel}", () => a.Factory(element.Value, reloaded))))
                    from checks in Combine.Checkrs(expectations.Select(a =>
                        Checkr.Expect($"'{name}' {a.label}", () => a.expectation(reloaded))))
                        // from failureChecks in Combine.Checkrs(
                        //     failures.Select(a => a.GetCheckr(client, httpMethod, element.Value, request)))
                    select Case.Closed)
                select Case.Closed);
    }
}
