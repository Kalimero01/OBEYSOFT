using System.Data.Common;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Obeysoft.Domain.Navigation;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class NavigationController : ControllerBase
    {
        private static readonly SemaphoreSlim SchemaLock = new(1, 1);

        private readonly BlogDbContext _db;
        private readonly ILogger<NavigationController> _logger;

        public NavigationController(BlogDbContext db, ILogger<NavigationController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public sealed record UpsertDto(string Label, string Href, Guid? ParentId, int DisplayOrder, bool IsActive);

        [HttpGet]
        public Task<IActionResult> List(CancellationToken ct)
            => ExecuteWithSchemaRecovery(async () =>
            {
                var list = await _db.NavigationItems
                    .AsNoTracking()
                    .OrderBy(x => x.ParentId)
                    .ThenBy(x => x.DisplayOrder)
                    .ToListAsync(ct);
                return Ok(list);
            }, ct);

        [HttpPost]
        public Task<IActionResult> Create([FromBody] UpsertDto dto, CancellationToken ct)
            => ExecuteWithSchemaRecovery(async () =>
            {
                var n = NavigationItem.Create(dto.Label, dto.Href, dto.ParentId, dto.DisplayOrder, dto.IsActive);
                _db.NavigationItems.Add(n);
                await _db.SaveChangesAsync(ct);
                return Ok(new { id = n.Id });
            }, ct);

        [HttpPut("{id:guid}")]
        public Task<IActionResult> Update(Guid id, [FromBody] UpsertDto dto, CancellationToken ct)
            => ExecuteWithSchemaRecovery(async () =>
            {
                var n = await _db.NavigationItems.FirstOrDefaultAsync(x => x.Id == id, ct);
                if (n is null) return NotFound();
                n.Update(dto.Label, dto.Href, dto.ParentId, dto.DisplayOrder, dto.IsActive);
                await _db.SaveChangesAsync(ct);
                return Ok();
            }, ct);

        [HttpDelete("{id:guid}")]
        public Task<IActionResult> Delete(Guid id, CancellationToken ct)
            => ExecuteWithSchemaRecovery(async () =>
            {
                var n = await _db.NavigationItems.FirstOrDefaultAsync(x => x.Id == id, ct);
                if (n is null) return NotFound();
                _db.NavigationItems.Remove(n);
                await _db.SaveChangesAsync(ct);
                return NoContent();
            }, ct);

        private async Task<IActionResult> ExecuteWithSchemaRecovery(Func<Task<IActionResult>> action, CancellationToken ct)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                if (!await TryRecoverFromMissingNavigationSchemaAsync(ex, ct))
                {
                    throw;
                }
            }

            return await action();
        }

        private async Task<bool> TryRecoverFromMissingNavigationSchemaAsync(Exception exception, CancellationToken ct)
        {
            if (!IsNavigationSchemaMissing(exception))
            {
                return false;
            }

            _logger.LogWarning(exception, "NavigationItems schema is missing. Triggering automatic migration.");

            try
            {
                await SchemaLock.WaitAsync(ct);
                try
                {
                    await _db.Database.MigrateAsync(ct);
                    _db.ChangeTracker.Clear();
                }
                finally
                {
                    SchemaLock.Release();
                }

                return true;
            }
            catch (Exception migrateEx)
            {
                _logger.LogError(migrateEx, "NavigationItems schema migration failed.");
                return false;
            }
        }

        private static bool IsNavigationSchemaMissing(Exception exception)
        {
            return exception switch
            {
                PostgresException pg when pg.SqlState is PostgresErrorCodes.UndefinedTable or PostgresErrorCodes.UndefinedColumn => true,
                DbException dbEx when IsMissingNavigationTableMessage(dbEx.Message) => true,
                InvalidOperationException invEx when IsMissingNavigationTableMessage(invEx.Message) => true,
                _ when exception.InnerException is not null => IsNavigationSchemaMissing(exception.InnerException),
                _ => false
            };
        }

        private static bool IsMissingNavigationTableMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            message = message.Trim();

            return message.Contains("NavigationItems", StringComparison.OrdinalIgnoreCase)
                && (message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("no such table", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("relation", StringComparison.OrdinalIgnoreCase) && message.Contains("does not exist", StringComparison.OrdinalIgnoreCase));
        }
    }
}


