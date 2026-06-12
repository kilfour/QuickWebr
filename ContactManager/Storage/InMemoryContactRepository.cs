using ContactManager.Core;

namespace ContactManager.Storage;

public class InMemoryContactRepository : IContactRepository
{
    private readonly List<Contact> contacts = [];
    private int lastId = 0;
    public void Add(Contact contact)
    {
        contact.Id = ++lastId;
        contacts.Add(contact);
    }
    public IReadOnlyList<Contact> GetAll() => contacts;

    public Contact? GetById(int id)
        => contacts.FirstOrDefault(c => c.Id == id);

    public bool Delete(int id)
    {
        var contact = contacts.FirstOrDefault(c => c.Id == id);
        if (contact is null) return false;
        contacts.Remove(contact);
        return true;
    }

    public IReadOnlyList<Contact> Search(string search)
        => [.. contacts.Where(c => c.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase))];

    public void Commit() { }
}