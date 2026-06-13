using System.Net;
using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders;

public class UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    RequestInfo<TPoolElement, TRequest> requestInfo,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElement, TRequest, TPoolElement> store,
    Func<TReader, TPoolElement, TDbValue> read)
{
    private readonly List<MethodFailure<TPoolElement, TRequest, TRouteId>> failures = [];

    public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> FailsWith(
        string label,
        HttpStatusCode statusCode,
        Func<TPoolElement, TRequest, (TPoolElement, TRequest)> mutate)
    {
        failures.Add(new MethodFailure<TPoolElement, TRequest, TRouteId>(statusCode, $"'{name}' {label}", mutate, getRouteId, routeFactory));
        return this;
    }

    public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> FailsWith(
        string label,
        HttpStatusCode statusCode,
        Func<TRequest, TRequest> mutate)
    {
        failures.Add(new MethodFailure<TPoolElement, TRequest, TRouteId>(statusCode, $"'{name}' {label}", (e, r) => (e, mutate(r)), getRouteId, routeFactory));
        return this;
    }

    private readonly List<(string TraceLabel, Func<TPoolElement, TRequest, TDbValue, object> Factory)> traces = [];
    public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> Trace(
        string traceLabel,
        Func<TPoolElement, TRequest, TDbValue, object> factory)
    {
        traces.Add((traceLabel, factory));
        return this;
    }

    public Specification<TReader> Expect(params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations)
        => new(TheCheckr(expectations));

    private Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> TheCheckr(
        params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations)
    {
        return (client, db) =>
            poolCondition.GetCheckr(name, element =>
                from route in Checkr.Capture(() => routeFactory(getRouteId(element.Value)))
                from traceRoute in Checkr.Trace("Route", () => route)
                from request in Checkr.Input($"'{name}' Request", requestInfo.Fuzzr, requestInfo.GetShrinkers(element.Value))
                from response in Checkr.ShrinkableAct(name,
                    () => Send<TRequest>.Request(client, httpMethod, route, request))
                from guard in Checkr.When(() => response.HasExecuted,
                    from responseIsSuccess in StatusCodeIs.Success(name, response)
                    from stored in element.Replace(store(element.Value, request))
                    from reloaded in Checkr.Capture(() => read(db, element.Value))
                    from traces in Combine.Checkrs(traces.Select(a =>
                        Checkr.Trace($"'{name}' {a.TraceLabel}", () => a.Factory(element.Value, request, reloaded))))
                    from checks in Combine.Checkrs(expectations.Select(a =>
                        Checkr.Expect($"'{name}' {a.label}", () => a.expectation(request, reloaded))))
                    from failureChecks in Combine.Checkrs(
                        failures.Select(a => a.GetCheckr(client, httpMethod, element.Value, request)))
                    select Case.Closed)
                select Case.Closed);
    }
}
