using LumenEstoque.DTOs.MovementsDTOs;
using LumenEstoque.Pagination;
using LumenEstoque.Services.MovementServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LumenEstoque.Controllers;

[Route("api/v1/movements")]
[ApiController]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movementService;

    public MovementsController(IMovementService movementService)
    {
        _movementService = movementService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedList<MovementReadDTO>>> GetAllAsync([FromQuery] MovementParameters movementParameters)
    {
        var movements = await _movementService.GetAllAsync(movementParameters);
        return CreatePaginatedResponse(movements);
    }

    [Authorize]
    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<MovementReadDTO>> GetByIdAsync(int id)
    {
        var movement = await _movementService.GetByIdAsync(id);
        return Ok(movement);
    }

    [Authorize]
    [HttpGet("product/{id:int:min(1)}")]
    public async Task<ActionResult<PagedList<MovementReadDTO>>> GetByProduct([FromRoute] int id, [FromQuery] MovementParameters movementParameters)
    {
        var movements = await _movementService.GetByProduct(id, movementParameters);
        return CreatePaginatedResponse(movements);
    }

    [Authorize]
    [HttpPost("in")]
    public async Task<ActionResult<MovementReadDTO>> InAsync([FromBody] MovementCreateDTO movementCreateDTO)
    {
        var movement = await _movementService.InAsync(movementCreateDTO);
        return Ok(movement);
    }

    [Authorize]
    [HttpPost("out")]
    public async Task<IActionResult> OutAsync([FromBody] MovementCreateDTO movementCreateDTO)
    {
        var movement = await _movementService.OutAsync(movementCreateDTO);
        return Ok(movement);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustAsync([FromBody] MovementCreateDTO movementCreateDTO)
    {
        var movement = await _movementService.AdjustAsync(movementCreateDTO);
        return Ok(movement);
    }

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
