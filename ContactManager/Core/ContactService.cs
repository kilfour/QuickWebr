using ContactManager.Core.CreateContact;
using ContactManager.Core.GetAll;
using ContactManager.Core.Search;
using ContactManager.Core.UpdateContact;
using ContactManager.Storage;


namespace ContactManager.Core;

public interface IContactService
{
    CreateContactResponse AddContact(CreateContactRequest createContactRequest);
    bool DeleteContact(int id);
    List<GetAllContactResponse> GetAll();
    List<SearchContactResponse> Search(string search);
    bool UpdateContact(int id, UpdateContactRequest request);
}

public class ContactService(IContactRepository repository) : IContactService
{
    public CreateContactResponse AddContact(CreateContactRequest createContactRequest)
    {
        var contact = new Contact(createContactRequest.Name);
        repository.Add(contact);
        return new CreateContactResponse { Id = contact.Id, Name = contact.Name };
    }

    public List<GetAllContactResponse> GetAll()
        => [.. repository.Search(string.Empty).Select(a => new GetAllContactResponse { Id = a.Id, Name = a.Name })];

    public bool UpdateContact(int id, UpdateContactRequest request)
    {
        var contact = repository.GetById(id);
        if (contact == null)
            return false;
        contact.Name = request.Name;
        repository.Commit();
        return true;
    }

    public bool DeleteContact(int id)
        => repository.Delete(id);

    public List<SearchContactResponse> Search(string search)
        => [.. repository.Search(search).Select(a => new SearchContactResponse { Id = a.Id, Name = a.Name })];
}