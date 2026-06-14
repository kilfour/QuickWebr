using System.Net;
using System.Net.Http.Json;
using QuickCheckr;
using QuickCheckr.Diagnostics;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.CreateBuilders;

public class CreateExpect<TReader, TRequest, TResponse, TPoolElement, TDbValue>(
    string name,
    string route,
    FuzzrOf<TRequest> fuzzr,
    Func<TResponse, bool> responseCheck,
    Func<IReadOnlyCollection<TPoolElement>, bool> predicate,
    Func<TRequest, TResponse, TPoolElement> toPool,
    Func<TReader, TPoolElement, TDbValue> read)
{
    private List<MethodNoIdFailure<TPoolElement, TRequest>> failures = [];
    public CreateExpect<TReader, TRequest, TResponse, TPoolElement, TDbValue> FailsWith(
        string label,
        HttpStatusCode statusCode,
        Func<TRequest, TRequest> mutate)
    {
        failures.Add(new MethodNoIdFailure<TPoolElement, TRequest>(statusCode, $"'{name}' {label}", route, mutate));
        return this;
    }

    public Specification<TReader> Expect(params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations)
        => new(TheCheckr(expectations));

    private Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> TheCheckr(
        params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations)
    {
        return (client, db) =>
            Trackr.PoolWhen(predicate,
                from traceRoute in Checkr.Trace("Route", () => route)
                from request in Checkr.Input($"'{name}' Request", fuzzr)
                from response in Checkr.ShrinkableAct(name, () => client.PostAsJsonAsync(route, request))
                from guard in Checkr.When(() => response.HasExecuted,
                    from responseIsSuccess in StatusCodeIs.Success(name, response)
                    from result in Checkr.Capture(() => response.Result.Content.ReadFromJsonAsync<TResponse>().Result)
                    from created in Checkr.Expect($"'{name}' Response", () => responseCheck(result))
                    from stored in Trackr.ToPool($"'{name}' to Pool", () => toPool(request, result))
                    from _ in Autopsy.Note("Create", () => $"{stored.GetType().Name}-{result}")
                    from reloaded in Checkr.Capture(() => read(db, stored))
                    from checks in Combine.Checkrs(expectations.Select(a =>
                        Checkr.Expect($"'{name}' {a.label}", () => a.expectation(request, reloaded))))
                    from failureChecks in Combine.Checkrs(
                        failures.Select(a => a.GetCheckr(client, HttpMethod.Post, request)))
                    select Case.Closed)
                select Case.Closed);
    }
}
