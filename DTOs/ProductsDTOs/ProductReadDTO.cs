using LumenEstoque.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LumenEstoque.DTOs.ProductsDTOs
{
    public class ProductReadDTO
    {
        public int Id { get; set; }

        public string? Sku { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal CostPrice { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int MinStock { get; set; }

        public string? Unit { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int CategoryId { get; set; }

        public int SupplierId { get; set; }

        public Category? Category { get; set; }

        public Supplier? Supplier { get; set; }

        public string? CategoryName { get; set; }

        public string? SupplierName { get; set; }

    }
}
