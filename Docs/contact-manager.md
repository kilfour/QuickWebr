# ContactManager Acceptance Tests

**WebApplicationFactory:**  
```csharp
public class WebrApplicationFactory : WebApplicationFactory<Program>
{
    public IContactRepository GetReader() => Services.GetRequiredService<IContactRepository>();
};
```

**The Webr:**  
```csharp
Webr.Named("Contact Manager")
    .Context(() => new WebrApplicationFactory())
    .Client(a => a.CreateClient())
    .Reader(a => a.GetReader())
    .Methods(
        new CreateContact(),
        new UpdateContact(),
        new DeleteContact(),
        new GetContacts(),
        new SearchContacts())
    .Observe();
```

**The Methods:**  
```csharp
public class CreateContact : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Create("Create Contact")
            .Always<ContactInfo>()
            .Route("api/contacts")
            .Send(Fuzzr.One<CreateContactRequest>())
            .ResponseIs<CreateContactResponse>(a => a is not null)
            .Store(response => new ContactInfo(response.Id, response.Name))
            .ReadBack((repo, info) => repo.GetById(info.Id))
            .Expect(("Name", (request, contact) => contact.Name == request.Name));
}
```
```csharp
public class UpdateContact : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Update("Update Contact", HttpMethod.Put)
            .When<ContactInfo>(a => a.Count > 0)
            .Route(info => info.Id, a => $"api/contacts/{a}")
            .Send(from req in Fuzzr.One<UpdateContactRequest>() select req)
            .Store((info, request) => info with { Name = request.Name })
            .ReadBack((repo, info) => repo.GetById(info.Id))
            .FailsWith("Not Found", HttpStatusCode.NotFound, (info, request) => (info with { Id = -1 }, request))
            .Expect(("Name", (request, contact) => contact.Name == request.Name));
}
```
```csharp
public class DeleteContact : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Delete("Delete Contact")
            .When<ContactInfo>(a => a.Count > 0)
            .Route(info => info.Id, a => $"api/contacts/{a}")
            .ReadBack((repo, info) => repo.GetAll())
            .FailsWith("Not Found", HttpStatusCode.NotFound, info => (info with { Id = -1 }))
            .Expect(("Deleted", (info, contacts) => !contacts.Any(a => a.Id == info.Id)));
}
```
```csharp
public class GetContacts : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Get("Get Contacts")
            .Always<ContactInfo>()
            .Route("api/contacts")
            .Send()
            .ResponseIs<IReadOnlyList<GetAllContactResponse>>()
            .ExpectAll("Contains All Stored Contacts",
                (response, info) => response.Any(contact => contact.Id == info.Id));
}
```
```csharp
public class SearchContacts : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Get("Search Contacts")
            .When<ContactInfo>(a => a.Count > 0)
            .Route("api/contacts/search")
            .SendQuery(info => ("name", info.Name))
            .ResponseIs<IReadOnlyList<SearchContactResponse>>()
            .When("Empty List", info => ("name", "nope-not-here"), (response, info) => !response.Any())
            .Expect("Contains Stored Contact",
                (response, info) => response.Any(contact => contact.Id == info.Id));
}
```

**The Report:**  
```text
------------------------------------------------------------
 5 Runs
 ------------------------------------------------------------
 Passed Expectations
 - 'Get Contacts' Status Code is Success: 27x
 - 'Get Contacts' Response: 27x
 - 'Create Contact' Status Code is Success: 24x
 - 'Create Contact' Response: 24x
 - 'Create Contact' Name: 24x
 - 'Get Contacts' Contains All Stored Contacts: 10x
 - 'Search Contacts' Status Code is Success: 15x
 - 'Search Contacts' Response: 15x
 - 'Search Contacts' Contains Stored Contact: 15x
 - 'Search Contacts' Empty List, Status Code: 5x
 - 'Delete Contact' Status Code is Success: 13x
 - 'Delete Contact' Deleted: 13x
 - 'Delete Contact' Not Found, Status Code: 5x
 - 'Update Contact' Status Code is Success: 21x
 - 'Update Contact' Name: 21x
 - 'Update Contact' Not Found, Status Code: 5x
 ------------------------------------------------------------
```
