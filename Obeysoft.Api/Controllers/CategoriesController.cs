// FILE: Obeysoft.Api/Controllers/CategoriesController.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Obeysoft.Application.Categories;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CategoriesController : ControllerBase
    {
        private readonly IGetCategoryService _service;

        public CategoriesController(IGetCategoryService service) => _service = service;

        /// <summary>Sol menü için tüm aktif kategoriler (flat liste).</summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(System.Collections.Generic.IReadOnlyList<GetCategoryListItemDto>), 200)]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var list = await _service.GetAllActiveAsync(ct);
            return Ok(list);
        }

        /// <summary>Aktif kategorilerin hiyerarşik ağaç görünümü (root → children).</summary>
        [HttpGet("tree")]
        [ProducesResponseType(typeof(System.Collections.Generic.IReadOnlyList<CategoryTreeNodeDto>), 200)]
        public async Task<IActionResult> GetTree(CancellationToken ct)
        {
            var tree = await _service.GetActiveTreeAsync(ct);
            return Ok(tree);
        }

        /// <summary>Belirli bir kategorinin (GUID) doğrudan çocukları.</summary>
        [HttpGet("{parentId:guid}/children")]
        [ProducesResponseType(typeof(System.Collections.Generic.IReadOnlyList<GetCategoryListItemDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetChildren(Guid parentId, CancellationToken ct)
        {
            if (parentId == Guid.Empty) return BadRequest();
            var list = await _service.GetChildrenAsync(parentId, ct);
            return Ok(list);
        }

        /// <summary>Slug ile kategori getir.</summary>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(GetCategoryListItemDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
        {
            var dto = await _service.GetBySlugAsync(slug, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        /// <summary>İsme/slug’a göre arama.</summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(System.Collections.Generic.IReadOnlyList<GetCategoryListItemDto>), 200)]
        public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
        {
            var list = await _service.SearchAsync(query, ct);
            return Ok(list);
        }
    }
}
