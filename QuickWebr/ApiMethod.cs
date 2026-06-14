using QuickWebr.Bolts.CreateBuilders;
using QuickWebr.Bolts.DeleteBuilders;
using QuickWebr.Bolts.GetBuilders;
using QuickWebr.Bolts.UpdateBuilders;

namespace QuickWebr;

public abstract class ApiMethod<TReader>
{
    protected static GetWhen<TReader> Get(string name) => new(name);

    protected static CreateWhen<TReader> Create(string name) => new(name);

    protected static UpdateWhen<TReader> Update(string name) => new(name, HttpMethod.Post);
    protected static UpdateWhen<TReader> Update(string name, HttpMethod httpMethod) => new(name, httpMethod);

    protected static DeleteWhen<TReader> Delete(string name) => new(name, HttpMethod.Delete);
    protected static DeleteWhen<TReader> Delete(string name, HttpMethod httpMethod) => new(name, httpMethod);

    public abstract Specification<TReader> Define();
}