using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Chat.Api.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation error", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Used by our app for "not found" or "invalid credentials"
            // We'll map it to 400 by default; controllers can still return 404 explicitly when needed.
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Request error", ex.Message);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Duplicate key", "Resource already exists.");
        }
        catch (Exception)
        {
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}