using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LumenEstoque.DTOs.ProductsDTOs;

namespace LumenEstoque.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "O SKU é obrigatório")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "O SKU deve ter entre 3 e 30 caracteres")]
    public string? Sku { get; set; }

    [Required(ErrorMessage = "O Nome é obrigatório")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O Nome deve ter entre 3 e 50 caracteres")]
    public string? Name { get; set; }

    [StringLength(200, ErrorMessage = "A Descrição pode ter no máximo 200 caracteres")]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 99999999.99, ErrorMessage = "O Preço de Custo deve ser um valor positivo com até 2 casas decimais")]
    public decimal CostPrice { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    [Range(0.01, 99999999.99, ErrorMessage = "O Preço deve ser um valor positivo com até 2 casas decimais")]
    public decimal Price { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque deve ser um valor inteiro não negativo")]
    public int StockQuantity { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "O estoque mínimo deve ser um valor inteiro não negativo")]
    public int MinStock { get; set; } = 5;

    [Required(ErrorMessage = "A Unidade é obrigatória")]
    [StringLength(5, MinimumLength = 1, ErrorMessage = "A Unidade deve ter entre 1 e 5 caracteres")]
    public string Unit { get; set; } = "un";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "A Categoria é obrigatória")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O Fornecedor é obrigatório")]
    public int SupplierId { get; set; }

    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public Category? Category { get; set; }

    [ForeignKey("SupplierId")]
    [JsonIgnore]
    public Supplier? Supplier { get; set; }

}