using Asp.Versioning;
using LumenEstoque.DTOs.CategoriesDTOs;
using LumenEstoque.Models;
using LumenEstoque.Pagination;
using LumenEstoque.Services.CategoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace LumenEstoque.Controllers;

[Route("api/v{version:apiVersion}/categories")]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "categories_cache";
    public CategoriesController(ICategoryService categoryService, IMemoryCache memoryCache)
    {
        _categoryService = categoryService;
        _cache = memoryCache;
    }

    /// <summary>
    /// Retorna uma lista paginada de categorias.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. Os parâmetros de paginação e filtro são passados via query string.
    /// Os metadados de paginação (TotalCount, PageSize, CurrentPage, TotalPages, HasNext, HasPrevious)
    /// são retornados no header <c>X-Pagination</c> da resposta.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/categories?PageNumber=1&amp;PageSize=20
    ///
    /// </remarks>
    /// <param name="categoryParameters">Parâmetros de paginação e filtro da consulta.</param>
    /// <returns>Uma lista paginada de categorias.</returns>
    //[Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<CategoryReadDTO>>> GetAllAsync([FromQuery] CategoryParameters categoryParameters)
    {
        if(!_cache.TryGetValue(CacheKey, out PagedList<CategoryReadDTO>? categories))
        {
            categories = await _categoryService.GetAllAsync(categoryParameters);

            if (categories != null && categories.Any())
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                    SlidingExpiration = TimeSpan.FromSeconds(15),
                    Priority = CacheItemPriority.High
                };
                _cache.Set(CacheKey, categories, cacheOptions);
            }
        }

        return CreatePaginatedResponse(categories);
    }

    /// <summary>
    /// Retorna uma categoria pelo seu identificador único.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. O <paramref name="id"/> deve ser um inteiro maior ou igual a 1.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/categories/3
    ///
    /// </remarks>
    /// <param name="id">Identificador único da categoria. Deve ser maior ou igual a 1.</param>
    /// <returns>A categoria correspondente ao <paramref name="id"/> informado.</returns>
    //[Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryReadDTO>> GetByIdAsync(int id)
    {
        var CacheKey = $"category_{id}";

        if (!_cache.TryGetValue($"{CacheKey}_{id}", out CategoryReadDTO? category))
        {
            category = await _categoryService.GetByIdAsync(id);

            if (category != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                    SlidingExpiration = TimeSpan.FromSeconds(15),
                    Priority = CacheItemPriority.High
                };
                _cache.Set(CacheKey, category, cacheOptions);
            }
        }

        return Ok(category);
    }

    /// <summary>
    /// Cria uma nova categoria.
    /// </summary>
    /// <remarks>
    /// Requer autenticação e perfil <c>Admin</c>.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/categories
    ///     {
    ///         "name": "Eletrônicos",
    ///         "description": "Produtos eletrônicos em geral"
    ///     }
    ///
    /// </remarks>
    /// <param name="categoryCreateDTO">Dados da categoria a ser criada.</param>
    /// <returns>A categoria recém-criada com seu identificador gerado.</returns>
    //[Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryReadDTO>> CreateAsync([FromBody] CategoryCreateDTO categoryCreateDTO)
    {
        var createdCategory = await _categoryService.CreateAsync(categoryCreateDTO);

        _cache.Remove(CacheKey);

        var cacheKey = $"category_{createdCategory.Id}";

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            SlidingExpiration = TimeSpan.FromSeconds(15),
            Priority = CacheItemPriority.High
        };

        _cache.Set(cacheKey, createdCategory, cacheOptions);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = createdCategory.Id }, createdCategory);
    }

    /// <summary>
    /// Atualiza os dados de uma categoria existente.
    /// </summary>
    /// <remarks>
    /// Requer autenticação e perfil <c>Admin</c>. O <paramref name="id"/> deve corresponder
    /// a uma categoria existente.
    ///
    /// Exemplo de requisição:
    ///
    ///     PUT /api/v1/categories/3
    ///     {
    ///         "name": "Eletrônicos Revisado",
    ///         "description": "Descrição atualizada"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">Identificador único da categoria a ser atualizada. Deve ser maior ou igual a 1.</param>
    /// <param name="categoryUpdateDTO">Dados atualizados da categoria.</param>
    /// <returns>A categoria com os dados atualizados.</returns>
    //[Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryReadDTO>> UpdateAsync([FromRoute] int id, [FromBody] CategoryUpdateDTO categoryUpdateDTO)
    {
        var updatedCategory = await _categoryService.UpdateAsync(id, categoryUpdateDTO);

        _cache.Set($"CacheCategoria_{id}", updatedCategory, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            SlidingExpiration = TimeSpan.FromSeconds(15),
            Priority = CacheItemPriority.High
        });

        _cache.Remove(CacheKey);

        return Ok(updatedCategory);
    }

    /// <summary>
    /// Remove uma categoria pelo seu identificador único.
    /// </summary>
    /// <remarks>
    /// Requer autenticação e perfil <c>Admin</c>. A operação é irreversível.
    ///
    ///     DELETE /api/v1/categories/3
    ///
    /// </remarks>
    /// <param name="id">Identificador único da categoria a ser removida. Deve ser maior ou igual a 1.</param>
    /// <returns>A categoria que foi removida.</returns>
    //[Authorize(Roles = "Admin")]
    [HttpDelete("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryReadDTO>> DeleteAsync(int id)
    {
        var deletedCategory = await _categoryService.DeleteAsync(id);

        _cache.Remove($"CacheCategoria_{id}");
        _cache.Remove(CacheKey);

        return Ok(deletedCategory);
    }

    private ActionResult<PagedList<CategoryReadDTO>> CreatePaginatedResponse(PagedList<CategoryReadDTO> categories)
    {
        var metadata = new
        {
            categories.TotalCount,
            categories.PageSize,
            categories.CurrentPage,
            categories.TotalPages,
            categories.HasNext,
            categories.HasPrevious
        };
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
        return Ok(categories);
    }
}