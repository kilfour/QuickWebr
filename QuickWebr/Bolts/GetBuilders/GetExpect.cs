using System.Net;
using System.Net.Http.Json;
using QuickCheckr;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickWebr.Bolts.GetBuilders;

public class GetExpect<TReader, TPoolElement, TResponse>(
    string name,
    Func<IReadOnlyCollection<TPoolElement>, bool> poolCondition,
    string route,
    Func<TPoolElement, (string, FuzzrOf<string>)>? queryValue,
    Func<TResponse, bool> responseCheck)
{

    public Specification<TReader> ExpectAll(string label, Func<TResponse, TPoolElement, bool> expectation) =>
        new((client, db) =>
            Trackr.PoolWhen(poolCondition,
                from traceRoute in Checkr.Trace("Route", () => route)
                from response in Checkr.ShrinkableAct(name, () => client.GetAsync(route))
                from responseIsSuccess in StatusCodeIs.Success(name, response)
                from result in Checkr.Capture(() => response.Result.Content.ReadFromJsonAsync<TResponse>())
                from check in Checkr.Expect($"'{name}' Response", () => responseCheck(result))
                from expect in Trackr.PoolExpectEach<TPoolElement>($"'{name}' {label}", (e) => expectation(result, e))
                select Case.Closed));

    private readonly List<MethodFailureWithQuery<TPoolElement>> failures = [];
    public GetExpect<TReader, TPoolElement, TResponse> FailsWith(
        string label,
        HttpStatusCode statusCode,
        Func<TPoolElement, (string, string)> mutate)
    {
        failures.Add(new MethodFailureWithQuery<TPoolElement>(statusCode, $"'{name}' {label}", route, mutate));
        return this;
    }

    private readonly List<Alternate<TPoolElement, TResponse>> alternates = [];
    public GetExpect<TReader, TPoolElement, TResponse> When(
        string label,
        Func<TPoolElement, (string, string)> mutate,
        Func<TResponse, TPoolElement, bool> expectation)
    {
        alternates.Add(new Alternate<TPoolElement, TResponse>($"'{name}' {label}", route, mutate, expectation));
        return this;
    }

    public GetExpect<TReader, TPoolElement, TResponse> When(
        string label,
        Func<TPoolElement, (string, string)> mutate,
        Func<TResponse, bool> expectation)
    {
        alternates.Add(new Alternate<TPoolElement, TResponse>($"'{name}' {label}", route, mutate, (r, e) => expectation(r)));
        return this;
    }

    public Specification<TReader> Expect(string label, Func<TResponse, TPoolElement, bool> expectation) =>
        new((client, db) =>
            Trackr.PoolWhen(name, poolCondition, element =>
                from actualRoute in queryValue is null
                    ? Checkr.Capture(() => route)
                    : from query in Checkr.Capture(() => queryValue(element.Value))
                      from value in Checkr.Input(query.Item1, query.Item2)
                      select $"{route}?{query.Item1}={value}"
                from traceRoute in Checkr.Trace("Route", () => actualRoute)
                from response in Checkr.ShrinkableAct(name, () => client.GetAsync(actualRoute))
                from responseIsSuccess in StatusCodeIs.Success(name, response)
                from result in Checkr.Capture(() => response.Result.Content.ReadFromJsonAsync<TResponse>())
                from check in Checkr.Expect($"'{name}' Response", () => responseCheck(result))
                from expect in Checkr.Expect($"'{name}' {label}", () => expectation(result, element.Value))
                from failuresCheck in Combine.Checkrs(failures.Select(a => a.GetCheckr(client, element.Value)))
                from alternatesCheck in Combine.Checkrs(alternates.Select(a => a.GetCheckr(client, element.Value)))
                select Case.Closed));
}