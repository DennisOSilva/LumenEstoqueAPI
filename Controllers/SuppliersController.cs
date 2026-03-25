using Asp.Versioning;
using LumenEstoque.DTOs.ProductsDTOs;
using LumenEstoque.DTOs.SuppliersDTOs;
using LumenEstoque.Pagination;
using LumenEstoque.Services.SupplierServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LumenEstoque.Controllers;

[Route("api/v{version:apiVersion}/suppliers")]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Retorna uma lista paginada de fornecedores.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. Os parâmetros de paginação e filtro são passados via query string.
    /// Os metadados de paginação (TotalCount, PageSize, CurrentPage, TotalPages, HasNext, HasPrevious)
    /// são retornados no header <c>X-Pagination</c> da resposta.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/suppliers?PageNumber=1&amp;PageSize=10
    ///
    /// </remarks>
    /// <param name="supplierParameters">Parâmetros de paginação e filtro da consulta.</param>
    /// <returns>Uma lista paginada de fornecedores.</returns>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<SupplierReadDTO>>> GetAllAsync([FromQuery] SupplierParameters supplierParameters)
    {
        var suppliers = await _supplierService.GetAllAsync(supplierParameters);
        return CreatePaginatedResponse(suppliers);
    }

    /// <summary>
    /// Retorna um fornecedor pelo seu identificador único.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. O <paramref name="id"/> deve ser um inteiro maior ou igual a 1.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/suppliers/5
    ///
    /// </remarks>
    /// <param name="id">Identificador único do fornecedor. Deve ser maior ou igual a 1.</param>
    /// <returns>O fornecedor correspondente ao <paramref name="id"/> informado.</returns>
    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult> GetByIdAsync(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        return Ok(supplier);
    }

    /// <summary>
    /// Cria um novo fornecedor.
    /// </summary>
    /// <remarks>
    /// Requer autenticação e perfil <c>Admin</c>.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/suppliers
    ///     {
    ///         "name": "Fornecedor ABC",
    ///         "cnpj": "12.345.678/0001-99",
    ///         "email": "contato@abc.com",
    ///         "phone": "(11) 91234-5678"
    ///     }
    ///
    /// </remarks>
    /// <param name="supplierCreateDTO">Dados do fornecedor a ser criado.</param>
    /// <returns>O fornecedor recém-criado com seu identificador gerado.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] SupplierCreateDTO supplierCreateDTO)
    {
        var createdSupplier = await _supplierService.CreateAsync(supplierCreateDTO);
        return Created($"api/v1/suppliers/{createdSupplier.Id}", createdSupplier);
    }

    /// <summary>
    /// Atualiza os dados de um fornecedor existente.
    /// </summary>
    /// <remarks>
    /// Requer autenticação e perfil <c>Admin</c>. O <paramref name="id"/> deve corresponder
    /// a um fornecedor existente.
    ///
    /// Exemplo de requisição:
    ///
    ///     PUT /api/v1/suppliers/5
    ///     {
    ///         "name": "Fornecedor ABC Atualizado",
    ///         "email": "novo@abc.com",
    ///         "phone": "(11) 99999-0000"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">Identificador único do fornecedor a ser atualizado. Deve ser maior ou igual a 1.</param>
    /// <param name="supplierUpdateDTO">Dados atualizados do fornecedor.</param>
    /// <returns>O fornecedor com os dados atualizados.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int:min(1)}")]
    public async Task<ActionResult> UpdateAsync(int id, [FromBody] SupplierUpdateDTO supplierUpdateDTO)
    {
        var updatedSupplier = await _supplierService.UpdateAsync(id, supplierUpdateDTO);
        return Ok(updatedSupplier);
    }

    /// <summary>
    /// Remove um fornecedor pelo seu identificador único.
    /// </summary>
    /// <remarks>
    /// Requer autenticação e perfil <c>Admin</c>. A operação é irreversível.
    ///
    ///     DELETE /api/v1/suppliers/5
    ///
    /// </remarks>
    /// <param name="id">Identificador único do fornecedor a ser removido. Deve ser maior ou igual a 1.</param>
    /// <returns>O fornecedor que foi removido.</returns>
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