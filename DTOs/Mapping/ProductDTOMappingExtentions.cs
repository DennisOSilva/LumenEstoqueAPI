using LumenEstoque.DTOs.ProductsDTOs;
using LumenEstoque.Models;

namespace LumenEstoque.DTOs.Mapping;

public static class ProductDTOMappingExtentions
{
    public static Product ToEntity(this ProductCreateDTO dto)
    {
        return new Product
        {
            Sku = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            CostPrice = dto.CostPrice,
            Price = dto.Price,
            Unit = dto.Unit,
            IsActive = dto.IsActive,
            MinStock = dto.MinStock,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId
        };
    }

    public static void ToUpdate(this Product product, ProductUpdateDTO dto)
    {
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CostPrice = dto.CostPrice;
        product.Price = dto.Price;
        product.Unit = dto.Unit;
        product.MinStock = dto.MinStock;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;
    }

    public static ProductReadDTO ToReadDTO(this Product product)
    {
        return new ProductReadDTO
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CostPrice = product.CostPrice,
            StockQuantity = product.StockQuantity,
            MinStock = product.MinStock,
            Unit = product.Unit,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId,
            CategoryName = product.Category?.Name,
            SupplierName = product.Supplier?.Name
        };
    }

    // LISTA de Produto -> LISTA de ReadDTO
    public static IEnumerable<ProductReadDTO> ToReadDTOs(this IEnumerable<Product> products)
    {
        if (products == null || !products.Any())
        {
            return Enumerable.Empty<ProductReadDTO>();
        }

        return products.Select(p => p.ToReadDTO());
    }
}
