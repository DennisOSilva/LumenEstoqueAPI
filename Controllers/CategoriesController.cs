using LumenEstoque.DTOs.CategoriesDTOs;
using LumenEstoque.Models;
using LumenEstoque.Pagination;
using LumenEstoque.Services.CategoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LumenEstoque.Controllers;

[Route("api/v1/categories")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<CategoryReadDTO>>> GetAllAsync([FromQuery] CategoryParameters categoryParameters)
    {
        var categories = await _categoryService.GetAllAsync(categoryParameters);
        return CreatePaginatedResponse(categories);
    }

    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryReadDTO>> GetByIdAsync(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryReadDTO>> CreateAsync([FromBody] CategoryCreateDTO categoryCreateDTO)
    {
        var createdCategory = await _categoryService.CreateAsync(categoryCreateDTO);
        return Created($"api/v1/categories/{createdCategory.Id}", createdCategory);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryReadDTO>> UpdateAsync([FromRoute] int id, [FromBody] CategoryUpdateDTO categoryUpdateDTO)
    {
        var updatedCategory = await _categoryService.UpdateAsync(id, categoryUpdateDTO);
        return Ok(updatedCategory);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryReadDTO>> DeleteAsync(int id)
    {
        var deletedCategory = await _categoryService.DeleteAsync(id);
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
