using ContactManager.Core;
using ContactManager.Core.CreateContact;
using ContactManager.Core.GetAll;
using ContactManager.Core.Search;
using ContactManager.Core.UpdateContact;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Api;

[ApiController]
[Route("api/contacts")]
public class ContactsController(IContactService service) : ControllerBase
{
    [HttpPost]
    public ActionResult<CreateContactResponse> Create(CreateContactRequest request)
    {
        var contact = service.AddContact(request);
        return Created($"/api/contacts/{contact.Id}", contact);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, UpdateContactRequest request)
        => service.UpdateContact(id, request) ? NoContent() : NotFound();


    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
        => service.DeleteContact(id) ? NoContent() : NotFound();

    [HttpGet]
    public ActionResult<List<GetAllContactResponse>> GetAll()
        => Ok(service.GetAll());

    [HttpGet("search")]
    public ActionResult<List<SearchContactResponse>> Search([FromQuery] string name)
        => Ok(service.Search(name));
}

