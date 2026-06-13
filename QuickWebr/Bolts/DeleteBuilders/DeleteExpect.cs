using System.Net;
using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.DeleteBuilders;

public class DeleteExpect<TReader, TPoolElement, TRouteId, TDbValue>(
    string name,
    HttpMethod httpMethod,
    PoolCondition<TPoolElement> poolCondition,
    Func<TPoolElement, TRouteId> getRouteId,
    Func<TRouteId, string> routeFactory,
    Func<TReader, TPoolElement, TDbValue> read)
{
    private List<MethodFailure<TPoolElement, TRouteId>> failures = [];
    public DeleteExpect<TReader, TPoolElement, TRouteId, TDbValue> FailsWith(
        string label,
        HttpStatusCode statusCode,
        Func<TPoolElement, TPoolElement> mutate)
    {
        failures.Add(new MethodFailure<TPoolElement, TRouteId>(statusCode, $"'{name}' {label}", mutate, getRouteId, routeFactory));
        return this;
    }

    public Specification<TReader> Expect(params (string label, Func<TPoolElement, TDbValue, bool> expectation)[] expectations)
        => new((client, db) =>
            poolCondition.GetCheckr(name, element =>
                from route in Checkr.Capture(() => routeFactory(getRouteId(element.Value)))
                from traceRoute in Checkr.Trace("Route", () => route)
                from response in Checkr.ShrinkableAct(name,
                    () => Send.Request(
                        client,
                        httpMethod,
                        route))
                from responseIsSuccess in StatusCodeIs.Success(name, response)
                from reloaded in Checkr.Capture(() => read(db, element.Value))
                from checks in Combine.Checkrs(expectations.Select(a =>
                    Checkr.Expect($"'{name}' {a.label}", () => a.expectation(element.Value, reloaded))))
                from failures in Combine.Checkrs(
                    failures.Select(a => a.GetCheckr(client, httpMethod, element.Value)))
                from store in element.Remove()
                select Case.Closed));
}