using LumenEstoque.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LumenEstoque.DTOs.ProductsDTOs
{
    public class ProductUpdateDTO
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O Nome deve ter entre 3 e 50 caracteres")]
        public string? Name { get; set; }

        [RegularExpression(@"^(\d{8}|\d{13})?$", ErrorMessage = "O EAN deve ter 8 ou 13 dígitos numéricos.")]
        public string? Ean { get; set; }

        [StringLength(200, ErrorMessage = "A Descrição pode ter no máximo 200 caracteres")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "O Preço de Custo deve ser um valor positivo com até 2 casas decimais")]
        public decimal CostPrice { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "O Preço deve ser um valor positivo com até 2 casas decimais")]
        public decimal Price { get; set; } = 0;

        [StringLength(5, MinimumLength = 1, ErrorMessage = "A Unidade deve ter entre 1 e 5 caracteres")]
        public string Unit { get; set; } = "un";

        [Range(1, int.MaxValue, ErrorMessage = "A Categoria é obrigatória")]
        public int? CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O Fornecedor é obrigatório")]
        public int? SupplierId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O estoque mínimo deve ser um valor inteiro não negativo")]
        public int? MinStock { get; set; } = 5;
    }
}
