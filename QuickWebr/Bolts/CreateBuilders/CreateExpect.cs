using System.Net.Http.Json;
using QuickCheckr;
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
    public Specification<TReader> Expect(params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations)
        => new(TheCheckr(expectations));

    private Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> TheCheckr(
        params (string label, Func<TRequest, TDbValue, bool> expectation)[] expectations)
    {
        return (client, db) =>
            Trackr.PoolWhen(predicate,
                from request in Checkr.Input($"'{name}' Request", fuzzr)
                from response in Checkr.ShrinkableAct(name, () => client.PostAsJsonAsync(route, request))
                from responseIsSuccess in StatusCodeIs.Success(name, response)
                from result in Checkr.Capture(() => response.Result.Content.ReadFromJsonAsync<TResponse>())
                from created in Checkr.Expect($"'{name}' Response", () => responseCheck(result.Result))
                from stored in Trackr.ToPool($"'{name}' to Pool", () => toPool(request, result.Result))
                from reloaded in Checkr.Capture(() => read(db, stored))
                from checks in Combine.Checkrs(expectations.Select(a =>
                    Checkr.Expect($"'{name}' {a.label}", () => a.expectation(request, reloaded))))
                select Case.Closed);
    }
}
