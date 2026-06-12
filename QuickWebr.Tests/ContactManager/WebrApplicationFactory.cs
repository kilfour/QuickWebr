using ContactManager.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuickPulse.Explains;

namespace QuickWebr.Tests.ContactManager;

[CodeExample]
public class WebrApplicationFactory : WebApplicationFactory<Program>
{
    public IContactRepository GetReader() => Services.GetRequiredService<IContactRepository>();
};
