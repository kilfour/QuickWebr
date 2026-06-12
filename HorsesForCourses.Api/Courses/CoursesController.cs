using HorsesForCourses.Api.Abstract;
using HorsesForCourses.Api.Service.Courses;
using Microsoft.AspNetCore.Mvc;

namespace HorsesForCourses.Api.Courses;

[ApiController]
[Route("courses")]
public class CoursesController(ICoursesService Service) : WebApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
        => Ok(await Service.CreateCourse(GetActor(), request.Name, request.StartDate, request.EndDate));

    [HttpPost("{id}/skills")]
    public async Task<IActionResult> UpdateRequiredSkills(int id, IEnumerable<string> skills)
        => NoContentNotFoundIfFalse(await Service.UpdateRequiredSkills(GetActor(), id, skills));

    [HttpPost("{id}/timeslots")]
    public async Task<IActionResult> UpdateTimeSlots(int id, IEnumerable<TimeSlotRequest> timeSlots)
        => NoContentNotFoundIfFalse(await Service.UpdateTimeSlots(GetActor(), id, timeSlots, a => (a.Day, a.Start, a.End)));

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmCourse(int id)
        => NoContentNotFoundIfFalse(await Service.ConfirmCourse(GetActor(), id));

    [HttpPost("{id}/assign-coach")]
    public async Task<IActionResult> AssignCoach(int id, AssignCoachRequest request)
        => NoContentNotFoundIfFalse(await Service.AssignCoach(GetActor(), id, request.CoachId));

    [HttpGet]
    public async Task<IActionResult> GetCourses(int page = 1, int pageSize = 25)
        => Ok(await Service.GetCourses(page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseDetail(int id)
        => OkNotFoundIfNull(await Service.GetCourseDetail(id));
}