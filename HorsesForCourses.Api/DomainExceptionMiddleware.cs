using HorsesForCourses.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace HorsesForCourses.Api;

public sealed class DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
{
    private readonly RequestDelegate next = next;
    private readonly ILogger<DomainExceptionMiddleware> logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogInformation(ex, "Domain rule violated");
            await WriteProblem(
                context,
                StatusCodes.Status400BadRequest,
                "Domain rule violated",
                ex.MessageFromType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblem(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                ex.Message);
        }
    }

    private static async Task WriteProblem(HttpContext ctx, int status, string title, string detail)
    {
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = status switch
            {
                StatusCodes.Status400BadRequest => "https://httpstatuses.com/400",
                StatusCodes.Status404NotFound => "https://httpstatuses.com/404",
                StatusCodes.Status409Conflict => "https://httpstatuses.com/409",
                _ => "about:blank"
            }
        };

        await ctx.Response.WriteAsJsonAsync(problem);
    }
}