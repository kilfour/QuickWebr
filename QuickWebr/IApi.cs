using Microsoft.EntityFrameworkCore;
using QuickWebr.Bolts.CreateBuilders;
using QuickWebr.Bolts.UpdateBuilders;

namespace QuickWebr;

public interface IApi : IDbAccess
{
    CreateBuilder Create(string route);
    UpdateBuilder Update(string route);
}
