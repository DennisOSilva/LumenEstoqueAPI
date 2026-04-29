using LumenEstoque.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LumenEstoque.Models;

public class Movement
{
    public int Id { get; set; }

    [Required]
    public MovementType Type { get; set; }

    [Range(1,int.MaxValue, ErrorMessage = "O valor tem que ser maior que 1")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "O valor tem que ser maior que 0")]
    public int PreviousStock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "O valor tem que ser maior que 0")]
    public int NewStock { get; set; }

    [StringLength(100, ErrorMessage = "A razão deve ter no máximo 100 caracteres")]
    public string? Reason { get; set; }

    public string? ReferenceDoc { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "O valor do ID tem que ser maior que 1")]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

}
