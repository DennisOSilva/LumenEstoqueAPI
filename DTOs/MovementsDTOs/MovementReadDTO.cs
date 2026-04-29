using LumenEstoque.Enums;

namespace LumenEstoque.DTOs.MovementsDTOs;

public class MovementReadDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceDoc { get; set; }
    public MovementType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProductName { get; set; }
}
