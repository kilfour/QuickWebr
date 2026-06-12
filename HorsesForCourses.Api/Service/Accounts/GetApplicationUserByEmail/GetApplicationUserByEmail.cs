using HorsesForCourses.Domain.Accounts;
using HorsesForCourses.Api.Service.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace HorsesForCourses.Api.Service.Accounts.GetApplicationUserByEmail;

public interface IGetApplicationUserByEmail
{
    Task<ApplicationUser?> One(string email);
}

public class GetApplicationUserByEmail : IGetApplicationUserByEmail
{
    private AppDbContext dbContext;

    public GetApplicationUserByEmail(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<ApplicationUser?> One(string email)
    {
        return await dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.Value == email);
    }
}