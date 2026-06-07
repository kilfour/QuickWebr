using System.Net.Http.Json;
using QuickCheckr;

namespace QuickWebr.Bolts.GetBuilders;

public class GetExpect<TReader, TPoolElement, TResponse>(
    string name,
    string route,
    Func<TResponse, bool> responseCheck)
{

    public Specification<TReader> Expect(string label, Func<TResponse, TPoolElement, bool> expectation)
        => new(TheCheckr(label, expectation));

    private Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> TheCheckr(
        string label, Func<TResponse, TPoolElement, bool> expectation)
    {
        return (client, db) =>
            Checkr.Option(() => true,
                from response in Checkr.ShrinkableAct(name, () => client.GetAsync(route))
                from responseIsSuccess in StatusCodeIs.Success(name, response)
                from result in Checkr.Capture(response.Result.Content.ReadFromJsonAsync<TResponse>())
                from check in Checkr.Expect($"'{name}' Response", () => responseCheck(result))
                from expect in Trackr.PoolExpectEach<TPoolElement>($"'{name}' {label}", (e) => expectation(result, e))
                select Case.Closed);
    }
}