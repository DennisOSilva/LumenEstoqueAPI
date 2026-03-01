using LumenEstoque.DTOs.ProductsDTOs;
using LumenEstoque.DTOs.SuppliersDTOs;
using LumenEstoque.Pagination;
using LumenEstoque.Services.SupplierServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LumenEstoque.Controllers;

[Route("api/v1/suppliers")]
[ApiController]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<SupplierReadDTO>>> GetAllAsync([FromQuery] SupplierParameters supplierParameters)
    {
        var suppliers = await _supplierService.GetAllAsync(supplierParameters);
        return CreatePaginatedResponse(suppliers);
    }

    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult> GetByIdAsync(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        return Ok(supplier);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] SupplierCreateDTO supplierCreateDTO)
    {
        var createdSupplier = await _supplierService.CreateAsync(supplierCreateDTO);
        return Created($"api/v1/suppliers/{createdSupplier.Id}", createdSupplier);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] SupplierUpdateDTO supplierUpdateDTO)
    {
        var updatedSupplier = await _supplierService.UpdateAsync(id, supplierUpdateDTO);
        return Ok(updatedSupplier);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var deletedSupplier = await _supplierService.DeleteAsync(id);
        return Ok(deletedSupplier);
    }

    private ActionResult<PagedList<SupplierReadDTO>> CreatePaginatedResponse(PagedList<SupplierReadDTO> suppliers)
    {
        var metadata = new
        {
            suppliers.TotalCount,
            suppliers.PageSize,
            suppliers.CurrentPage,
            suppliers.TotalPages,
            suppliers.HasNext,
            suppliers.HasPrevious
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(suppliers);
    }
}
