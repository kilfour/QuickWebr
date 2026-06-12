using System.Text.Json.Serialization;
using HorsesForCourses.Api.Service.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace HorsesForCourses.Api;

public static class BuilderExtensions
{
    public static IServiceCollection AddHorsesForCoursesWebApi(this IServiceCollection services)
        => services

            .AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=horsesforcourses.db"))

            .AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(null, allowIntegerValues: false)))
            .Services;
}