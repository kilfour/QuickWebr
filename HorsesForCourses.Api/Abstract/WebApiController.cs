using HorsesForCourses.Domain.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace HorsesForCourses.Api.Abstract;

public abstract class WebApiController : ControllerBase
{
    protected IActionResult OkNotFoundIfNull<T>(T value)
    {
        if (value == null) return NotFound();
        return Ok(value);
    }
    protected IActionResult NoContentNotFoundIfFalse(bool flag)
    {
        if (!flag) return NotFound();
        return NoContent();
    }

    protected Actor GetActor() => Actor.From(User?.Claims! ?? []);
}


