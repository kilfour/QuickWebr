using Microsoft.EntityFrameworkCore;

namespace QuickWebr;

public interface IDbAccess
{
    TDbValue Query<TDbValue>(Func<DbContext, TDbValue> load);
}
