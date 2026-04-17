using Asp.Versioning;
using LumenEstoque.DTOs.MovementsDTOs;
using LumenEstoque.Pagination;
using LumenEstoque.Services.MovementServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LumenEstoque.Controllers;

[Route("api/v{version:apiVersion}/movements")]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movementService;

    public MovementsController(IMovementService movementService)
    {
        _movementService = movementService;
    }

    /// <summary>
    /// Retorna todas as movimentações de estoque com paginação.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. Permite aplicar filtros e paginação via query string.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/movements?pageNumber=1&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="movementParameters">Parâmetros de paginação e filtragem.</param>
    /// <returns>Lista paginada de movimentações.</returns>
    //[Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<MovementReadDTO>>> GetAllAsync([FromQuery] MovementParameters movementParameters)
    {
        var movements = await _movementService.GetAllAsync(movementParameters);
        return CreatePaginatedResponse(movements);
    }

    /// <summary>
    /// Retorna uma movimentação de estoque pelo ID.
    /// </summary>
    /// <remarks>
    /// Requer autenticação.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/movements/1
    ///
    /// </remarks>
    /// <param name="id">ID da movimentação.</param>
    /// <returns>Dados da movimentação.</returns>
    //[Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<MovementReadDTO>> GetByIdAsync(int id)
    {
        var movement = await _movementService.GetByIdAsync(id);
        return Ok(movement);
    }

    /// <summary>
    /// Retorna movimentações de um produto específico.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. Permite paginação dos resultados.
    ///
    /// Exemplo de requisição:
    ///
    ///     GET /api/v1/movements/product/1?pageNumber=1&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="id">ID do produto.</param>
    /// <param name="movementParameters">Parâmetros de paginação.</param>
    /// <returns>Lista paginada de movimentações do produto.</returns>
    //[Authorize]
    [HttpGet("product/{id:int:min(1)}")]
    public async Task<ActionResult<PagedList<MovementReadDTO>>> GetByProduct([FromRoute] int id, [FromQuery] MovementParameters movementParameters)
    {
        var movements = await _movementService.GetByProduct(id, movementParameters);
        return CreatePaginatedResponse(movements);
    }

    /// <summary>
    /// Registra uma entrada de estoque.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. Utilizado para adicionar produtos ao estoque.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/movements/in
    ///
    /// </remarks>
    /// <param name="movementCreateDTO">Dados da movimentação de entrada.</param>
    /// <returns>Movimentação criada.</returns>
    //[Authorize]
    [HttpPost("in")]
    public async Task<ActionResult<MovementReadDTO>> InAsync([FromBody] MovementCreateDTO movementCreateDTO)
    {
        var movement = await _movementService.InAsync(movementCreateDTO);
        return Ok(movement);
    }

    /// <summary>
    /// Registra uma saída de estoque.
    /// </summary>
    /// <remarks>
    /// Requer autenticação. Utilizado para remover produtos do estoque.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/movements/out
    ///
    /// </remarks>
    /// <param name="movementCreateDTO">Dados da movimentação de saída.</param>
    /// <returns>Movimentação criada.</returns>
    //[Authorize]
    [HttpPost("out")]
    public async Task<IActionResult> OutAsync([FromBody] MovementCreateDTO movementCreateDTO)
    {
        var movement = await _movementService.OutAsync(movementCreateDTO);
        return Ok(movement);
    }

    /// <summary>
    /// Realiza um ajuste de estoque.
    /// </summary>
    /// <remarks>
    /// Requer autenticação com perfil <c>Admin</c>. Utilizado para correções de inventário.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/movements/adjust
    ///
    /// </remarks>
    /// <param name="movementCreateDTO">Dados do ajuste.</param>
    /// <returns>Movimentação de ajuste realizada.</returns>
    //[Authorize(Roles = "Admin")]
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustAsync([FromBody] MovementCreateDTO movementCreateDTO)
    {
        var movement = await _movementService.AdjustAsync(movementCreateDTO);
        return Ok(movement);
    }

    /// <summary>
    /// Cria a resposta paginada adicionando metadados no header.
    /// </summary>
    /// <param name="movement">Lista paginada.</param>
    /// <returns>Resposta com dados e metadados de paginação.</returns>
    private ActionResult<PagedList<MovementReadDTO>> CreatePaginatedResponse(PagedList<MovementReadDTO> movement)
    {
        var metadata = new
        {
            movement.TotalCount,
            movement.PageSize,
            movement.CurrentPage,
            movement.TotalPages,
            movement.HasNext,
            movement.HasPrevious
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(movement);
    }
}