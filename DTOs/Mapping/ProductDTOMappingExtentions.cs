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
            Ean = dto.Ean,
            Name = dto.Name,
            Description = dto.Description,
            CostPrice = dto.CostPrice,
            Price = dto.Price,
            Unit = dto.Unit,
            IsActive = dto.IsActive,
            Url = dto.Url,
            MinStock = dto.MinStock,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId
        };
    }

    public static void ToUpdate(this Product product, ProductUpdateDTO dto)
    {
        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Unit != null) product.Unit = dto.Unit;
        if (dto.Url != null) product.Url = dto.Url;
        if (!string.IsNullOrEmpty(dto.Ean) && string.IsNullOrEmpty(product.Ean))
            product.Ean = dto.Ean;

        product.CostPrice = dto.CostPrice ?? product.CostPrice;
        product.Price = dto.Price ?? product.Price;
        product.MinStock = dto.MinStock ?? product.MinStock;
        product.CategoryId = dto.CategoryId ?? product.CategoryId;
        product.SupplierId = dto.SupplierId ?? product.SupplierId;
    }
    public static ProductReadDTO ToReadDTO(this Product product)
    {
        return new ProductReadDTO
        {
            Id = product.Id,
            Ean = product.Ean,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CostPrice = product.CostPrice,
            StockQuantity = product.StockQuantity,
            MinStock = product.MinStock,
            Unit = product.Unit,
            IsActive = product.IsActive,
            Url = product.Url,
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
