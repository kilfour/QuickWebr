using the following method results in a course/0/skills route in the report

[CodeExample]
public class UpdateCourseSkills : ApiMethod<EfReader>
{
    public override Specification<EfReader> Define() =>
        Update("Update Course Skills")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .Route(info => info.Id, a => $"/courses/{a}/skills")
            .Send(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select new UpdateSkillsRequest([.. skills]))
            .Store((info, request) => info)
            .ReadBack((reader, info) => reader.Query(db => db.Find<Course>(Id<Course>.From(info.Id))))
            .Trace("req", (info, req, course) => info)
            .Expect(("Skills", (request, course) => Skills.Equal(request.Skills, course.RequiredSkills)));
}