// FILE: Obeysoft.Api/Middlewares/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Obeysoft.Api.Middlewares
{
    /// <summary>
    /// Tüm hataları tek noktada yakalar ve RFC7807 ProblemDetails JSON döner.
    /// - FluentValidation.ValidationException => 400 + errors
    /// - Yetkilendirme dışı burada ele alınmaz (401/403 pipeline tarafından)
    /// - EF/Unique ihlalleri => 409
    /// - Diğer tüm beklenmeyen hatalar => 500 (genel problem)
    /// </summary>
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException vex)
            {
                // 400 - Validation hataları (FluentValidation)
                _logger.LogWarning(vex, "Validation failed.");

                var errors = vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                await WriteProblem(
                    context,
                    status: (int)HttpStatusCode.BadRequest,
                    title: "One or more validation errors occurred.",
                    detail: "The request contains invalid data.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    extensions: new Dictionary<string, object?>
                    {
                        ["errors"] = errors
                    });
            }
            catch (DbUpdateException dbex) when (IsUniqueViolation(dbex))
            {
                // 409 - Unique ihlali (ör. Email, Slug)
                _logger.LogWarning(dbex, "Conflict (unique violation).");

                await WriteProblem(
                    context,
                    status: (int)HttpStatusCode.Conflict,
                    title: "Conflict",
                    detail: "The resource violates a unique constraint.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.8");
            }
            catch (Exception ex)
            {
                // 500 - Beklenmeyen hatalar
                _logger.LogError(ex, "Unhandled exception.");

                await WriteProblem(
                    context,
                    status: (int)HttpStatusCode.InternalServerError,
                    title: "An unexpected error occurred.",
                    detail: "The server encountered an unexpected error.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.6.1");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            // Npgsql benzersiz kısıt kodu: 23505
            if (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
                return true;

            // Başka provider’lar için genişletilebilir
            return false;
        }

        private static async Task WriteProblem(
            HttpContext ctx,
            int status,
            string title,
            string detail,
            string type,
            Dictionary<string, object?>? extensions = null)
        {
            if (ctx.Response.HasStarted) return;

            var problem = new Dictionary<string, object?>
            {
                ["type"] = type,
                ["title"] = title,
                ["status"] = status,
                ["detail"] = detail,
                ["instance"] = ctx.Request?.Path.Value,
                ["traceId"] = ctx.TraceIdentifier
            };

            if (extensions is not null)
            {
                foreach (var kv in extensions)
                    problem[kv.Key] = kv.Value;
            }

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json; charset=utf-8";

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await ctx.Response.WriteAsync(json);
        }
    }
}