using HorsesForCourses.Api.Service.Accounts;
using HorsesForCourses.Api.Service.Accounts.GetApplicationUserByEmail;
using HorsesForCourses.Api.Service.Coaches.GetCoachById;
using HorsesForCourses.Api.Service.Warehouse;
using HorsesForCourses.Api.Service.Coaches;
using HorsesForCourses.Api.Service.Coaches.GetCoachDetail;
using HorsesForCourses.Api.Service.Coaches.GetCoaches;
using HorsesForCourses.Api.Service.Courses;
using HorsesForCourses.Api.Service.Courses.GetCourseById;
using HorsesForCourses.Api.Service.Courses.GetCourseDetail;
using HorsesForCourses.Api.Service.Courses.GetCourses;

namespace HorsesForCourses.Api.Service;

public static class BuilderExtensions
{
    public static IServiceCollection AddHorsesForCourses(this IServiceCollection services)
        => services
            // === Common === 
            .AddScoped<IAmASuperVisor, DataSupervisor>()
            .AddScoped<IGetCoachById, GetCoachById>()
            // === Coaches === 
            .AddScoped<IGetCoachSummaries, GetCoachSummaries>()
            .AddScoped<IGetCoachDetailQuery, GetCoachDetailQuery>()
            .AddScoped<ICoachesService, CoachesService>()
            // === Courses === 
            .AddScoped<IGetCourseById, GetCourseById>()
            .AddScoped<IGetCourseSummaries, GetCourseSummaries>()
            .AddScoped<IGetCourseDetail, GetCourseDetail>()
            .AddScoped<ICoursesService, CoursesService>()
            // === Accounts === 
            .AddScoped<IGetApplicationUserByEmail, GetApplicationUserByEmail>()
            .AddScoped<IAccountsService, AccountsService>();
}