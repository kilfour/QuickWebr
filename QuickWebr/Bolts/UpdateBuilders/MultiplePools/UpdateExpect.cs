using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.UpdateBuilders.MultiplePools;

public class UpdateExpect<TReader, TPoolElementOne, TPoolElementTwo, TRequest, TRouteId, TDbValueOne, TDbValueTwo>(
    string name,
    HttpMethod httpMethod,
    Func<TReader, TPoolElementOne, TPoolElementTwo, bool> predicate,
    Func<TPoolElementOne, TPoolElementTwo, FuzzrOf<TRequest>> fuzzrFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TPoolElementOne, TPoolElementTwo, TRequest, TPoolElementOne> store,
    Func<TReader, TPoolElementOne, TPoolElementTwo, (TDbValueOne, TDbValueTwo)> read)
{
    // private readonly List<MethodFailure<TPoolElement, TRequest, TRouteId>> failures = [];

    // public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> FailsWith(
    //     string label,
    //     HttpStatusCode statusCode,
    //     Func<TPoolElement, TRequest, (TPoolElement, TRequest)> mutate)
    // {
    //     failures.Add(new MethodFailure<TPoolElement, TRequest, TRouteId>(statusCode, $"'{name}' {label}", mutate, getRouteId, routeFactory));
    //     return this;
    // }

    // public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> FailsWith(
    //     string label,
    //     HttpStatusCode statusCode,
    //     Func<TRequest, TRequest> mutate)
    // {
    //     failures.Add(new MethodFailure<TPoolElement, TRequest, TRouteId>(statusCode, $"'{name}' {label}", (e, r) => (e, mutate(r)), getRouteId, routeFactory));
    //     return this;
    // }

    // private readonly List<(string TraceLabel, Func<TPoolElement, TRequest, TDbValue, object> Factory)> traces = [];
    // public UpdateExpect<TReader, TPoolElement, TRequest, TRouteId, TDbValue> Trace(
    //     string traceLabel,
    //     Func<TPoolElement, TRequest, TDbValue, object> factory)
    // {
    //     traces.Add((traceLabel, factory));
    //     return this;
    // }

    public Specification<TReader> Expect(params (string label, Func<TRequest, TDbValueOne, TDbValueTwo, bool> expectation)[] expectations)
        => new(TheCheckr(expectations));

    private Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> TheCheckr(
        params (string label, Func<TRequest, TDbValueOne, TDbValueTwo, bool> expectation)[] expectations)
    {
        return (client, db) =>
            Trackr.PoolWhen<TPoolElementOne, TPoolElementTwo>(name, (a, b) => predicate(db, a, b), (elementOne, elementTwo) =>
                from route in Checkr.Capture(() => routeFactory(getRouteId(elementOne.Value, elementTwo.Value)))
                from traceRoute in Checkr.Trace("Route", () => route)
                from request in Checkr.Input($"'{name}' Request", fuzzrFactory(elementOne.Value, elementTwo.Value))
                from response in Checkr.ShrinkableAct(name,
                    () => Send<TRequest>.Request(client, httpMethod, route, request))
                from guard in Checkr.When(() => response.HasExecuted,
                    from responseIsSuccess in StatusCodeIs.Success(name, response)
                    from _ in Autopsy.Note("((el1.Id, el1.Value), (el2.Id, el2.Value))",
                        () => ((elementOne.Id, elementOne.Value), (elementTwo.Id, elementTwo.Value)))
                    from stored in elementOne.Replace(store(elementOne.Value, elementTwo.Value, request))
                    from reloaded in Checkr.Capture(() => read(db, elementOne.Value, elementTwo.Value))
                        // from traces in Combine.Checkrs(traces.Select(a =>
                        //     Checkr.Trace($"'{name}' {a.TraceLabel}", () => a.Factory(element.Value, request, reloaded))))
                    from checks in Combine.Checkrs(expectations.Select(a =>
                        Checkr.Expect($"'{name}' {a.label}", () => a.expectation(request, reloaded.Item1, reloaded.Item2))))
                        // from failureChecks in Combine.Checkrs(
                        //     failures.Select(a => a.GetCheckr(client, httpMethod, element.Value, request)))
                    select Case.Closed)
                select Case.Closed);
    }
}
