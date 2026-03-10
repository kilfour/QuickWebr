using QuickCheckr;

namespace QuickWebr;

public abstract class ApiMethod
{
    public abstract CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api);
}