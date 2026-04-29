using LumenEstoque.DTOs.MovementsDTOs;
using LumenEstoque.Models;

namespace LumenEstoque.DTOs.Mapping;

public static class MovementDTOMappingExtensions
{
    public static Movement ToEntity(this MovementCreateDTO dto, Product product)
    {

        return new Movement
        {
            Quantity = dto.Quantity,
            Reason = dto.Reason,
            ReferenceDoc = dto.ReferenceDoc,
            ProductId = dto.ProductId
        };
    }

    public static MovementReadDTO ToReadDTO(this Movement movement)
    {
        return new MovementReadDTO
        {
            Id = movement.Id,
            Type = movement.Type,
            Quantity = movement.Quantity,
            PreviousStock = movement.PreviousStock,
            NewStock = movement.NewStock,
            Reason = movement.Reason,
            ReferenceDoc = movement.ReferenceDoc,
            ProductId = movement.ProductId,
            CreatedAt = movement.CreatedAt,
            ProductName = movement.Product?.Name
        };
    }

    public static IEnumerable<MovementReadDTO> ToReadDTOs(this IEnumerable<Movement> movements)
    {
        if (movements == null || !movements.Any())
            return Enumerable.Empty<MovementReadDTO>();

        return movements.Select(m => m.ToReadDTO());
    }
}