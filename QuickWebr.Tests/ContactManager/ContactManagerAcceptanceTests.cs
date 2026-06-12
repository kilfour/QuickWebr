using System.Net;
using ContactManager.Api;
using ContactManager.Core.CreateContact;
using ContactManager.Core.GetAll;
using ContactManager.Core.Search;
using ContactManager.Core.UpdateContact;
using ContactManager.Storage;
using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickWebr.Bolts.WebrBuilders;
using QuickWebr.Tests.Tools;


namespace QuickWebr.Tests.ContactManager;

[DocFile]
[DocFileHeader("Contact Manager Acceptance Tests")]
[DocBoldHeader("WebApplicationFactory")]
[DocExample(typeof(WebrApplicationFactory))]
[DocWebrHeader]
[DocWebr]
[DocMethodsHeader]
[DocExample(typeof(CreateContact))]
[DocExample(typeof(UpdateContact))]
[DocExample(typeof(DeleteContact))]
[DocExample(typeof(GetContacts))]
[DocExample(typeof(SearchContacts))]
[DocReportHeader]
[DocReport]
public class ContactManagerAcceptanceTests : WebrRunTest<ContactManagerAcceptanceTests>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public void Example() =>
        Document(a => a.Run(10.Runs(), 50.ExecutionsPerRun()), Verify);

    [CodeSnippet]
    protected override ConfiguredCheckr GetWebr() =>
        Webr.Named("Contact Manager")
            .Context(() => new WebrApplicationFactory())
            .Client(a => a.CreateClient())
            .Reader(a => a.GetReader())
            .Methods(
                new CreateContact(),
                new UpdateContact(),
                new DeleteContact(),
                new GetContacts(),
                new SearchContacts());

    private void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(16, article.Total().PassedExpectations());
        var findings = article.Record as Findings;
        Assert.NotNull(findings);
        var passedExpectations = findings.PassedExpectationDepositions.Select(a => a.Label).ToList();
        Assert.Contains("'Create Contact' Status Code is Success", passedExpectations);
        Assert.Contains("'Create Contact' Response", passedExpectations);
        Assert.Contains("'Create Contact' Name", passedExpectations);
        Assert.Contains("'Update Contact' Status Code is Success", passedExpectations);
        Assert.Contains("'Update Contact' Name", passedExpectations);
        Assert.Contains("'Update Contact' Not Found, Status Code", passedExpectations);
        Assert.Contains("'Search Contacts' Status Code is Success", passedExpectations);
        Assert.Contains("'Search Contacts' Response", passedExpectations);
        Assert.Contains("'Search Contacts' Contains Stored Contact", passedExpectations);
        Assert.Contains("'Search Contacts' Empty List, Status Code", passedExpectations);
        Assert.Contains("'Delete Contact' Status Code is Success", passedExpectations);
        Assert.Contains("'Delete Contact' Deleted", passedExpectations);
        Assert.Contains("'Delete Contact' Not Found, Status Code", passedExpectations);
        Assert.Contains("'Get Contacts' Status Code is Success", passedExpectations);
        Assert.Contains("'Get Contacts' Response", passedExpectations);
        Assert.Contains("'Get Contacts' Contains All Stored Contacts", passedExpectations);
    }
}

public record ContactInfo(int Id, string Name);

[CodeExample]
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

[CodeExample]
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

[CodeExample]
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

[CodeExample]
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

[CodeExample]
public class SearchContacts : ApiMethod<IContactRepository>
{
    public override Specification<IContactRepository> Define() =>
        Get("Search Contacts")
            .When<ContactInfo>(a => a.Count > 0)
            .Route("api/contacts/search")
            .SendQuery(info => ("name", Fuzzr.Constant(info.Name)))
            .ResponseIs<IReadOnlyList<SearchContactResponse>>()
            .When("Empty List", info => ("name", "nope-not-here"), (response, info) => !response.Any())
            .Expect("Contains Stored Contact",
                (response, info) => response.Any(contact => contact.Id == info.Id));
}