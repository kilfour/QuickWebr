using System.Security.Claims;
using HorsesForCourses.Api.Service.Accounts.GetApplicationUserByEmail;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Domain.Accounts;
using HorsesForCourses.Domain.Accounts.InvalidationReasons;
using HorsesForCourses.Domain.Coaches;
using HorsesForCourses.InfraStructure;

namespace HorsesForCourses.Api.Service.Accounts;

public interface IAccountsService
{
    Task<bool> Register(string name, string email, string pass, bool asCoach, bool asAdmin);
    Task<IEnumerable<Claim>> Login(string coachEmail, string password);
}


public class AccountsService : IAccountsService
{
    private IAmASuperVisor supervisor;
    private readonly IGetApplicationUserByEmail getApplicationUserByEmail;

    public AccountsService(IAmASuperVisor supervisor, IGetApplicationUserByEmail getApplicationUserByEmail)
    {
        this.supervisor = supervisor;
        this.getApplicationUserByEmail = getApplicationUserByEmail;
    }

    public Task<bool> Register(string name, string email, string pass, bool asCoach, bool asAdmin)
    {
        var role = asAdmin ? ApplicationUser.AdminRole : asCoach ? ApplicationUser.CoachRole : string.Empty;
        supervisor.Enlist(ApplicationUser.Create(name, email, pass, role, new Pbkdf2PasswordHasher()));
        if (role == ApplicationUser.CoachRole)
            supervisor.Enlist(Coach.Create(Actor.SystemActor(), name, email));
        supervisor.Ship();
        return Task.FromResult(true);
    }

    public async Task<IEnumerable<Claim>> Login(string email, string password)
    {
        var applicationUser = await getApplicationUserByEmail.One(email);
        if (applicationUser == null)
            throw new EmailOrPasswordAreInvalid();
        applicationUser.CheckPassword(password, new Pbkdf2PasswordHasher());
        return await Task.FromResult(applicationUser.EnterScene().ClaimIt());
    }
}
