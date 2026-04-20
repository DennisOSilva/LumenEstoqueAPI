using LumenEstoque.Context;
using LumenEstoque.DTOs.Mapping;
using LumenEstoque.DTOs.ProductsDTOs;
using LumenEstoque.Enums;
using LumenEstoque.Models;
using LumenEstoque.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LumenEstoque.Services.ProductServices;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<ProductReadDTO?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return null;
        }

        return product.ToReadDTO();
    }

    public async Task<PagedList<ProductReadDTO>> GetAllAsync(ProductParameters productParameters)
    {
        IQueryable<Product> query = _context.Products!
            .Include(p => p.Supplier)
            .Include(p => p.Category);

        if (!string.IsNullOrEmpty(productParameters.search))
            query = query.Where(p => p.Name.Contains(productParameters.search) ||
                                     p.Sku.Contains(productParameters.search) ||
                                     p.Ean.Contains(productParameters.search));

        if (productParameters.categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == productParameters.categoryId);
        }
            
        if(productParameters.zeroStock == true)
        {
            query = query.Where(p => p.StockQuantity == 0);
        }
        else if (productParameters.lowStock == true)
        {
            query = query.Where(p => p.StockQuantity <= p.MinStock);
        }
            

        query = query.Where(p => p.IsActive == productParameters.active);

        query = productParameters.orderBy switch
        {
            ProductOrderBy.Name => productParameters.direction == OrderDirection.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            ProductOrderBy.Price => productParameters.direction == OrderDirection.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            ProductOrderBy.CreatedAt => productParameters.direction == OrderDirection.Descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var products = await PagedList<Product>.ToPagedListAsync(query, productParameters.PageNumber, productParameters.PageSize);

        if (!products.Any())
        {
            return new PagedList<ProductReadDTO>(new List<ProductReadDTO>(), 0, productParameters.PageNumber, productParameters.PageSize);
        }

        var productsDto = products.ToReadDTOs().ToList();
        return new PagedList<ProductReadDTO>(productsDto, products.TotalCount, products.CurrentPage, products.PageSize);
    }

    public async Task<ProductReadDTO> CreateAsync(ProductCreateDTO productCreateDTO)
    {
        var product = productCreateDTO.ToEntity();
        var products = await _context.Products!.ToListAsync();

        if (product == null)
        {
            throw new ArgumentException("Erro ao converter ProductCreateDTO para Product");
        }

        if (!string.IsNullOrEmpty(product.Sku))
        {
            var skuExiste = await _context.Products!.AnyAsync(p => p.Sku == product.Sku);
            if (skuExiste)
            {
                throw new ArgumentException("Já existe um produto com esse código SKU");
            }   

            product.Sku = product.Sku.ToUpper();
        }

        if (!string.IsNullOrEmpty(product.Ean))
        {
            var eanExiste = await _context.Products!.AnyAsync(p => p.Ean == product.Ean);
            if (eanExiste)
            {
                throw new ArgumentException("Já existe um produto com esse código de barras");
            }     
        }

        await _context.Products!.AddAsync(product);
        await _context.SaveChangesAsync();
        return product.ToReadDTO();
    }

    public async Task<ProductReadDTO> UpdateAsync(int id, ProductUpdateDTO productUpdateDTO)
    {
        var product = await _context.Products!.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }

        if (!string.IsNullOrEmpty(product.Ean) && !string.IsNullOrEmpty(productUpdateDTO.Ean) && product.Ean != productUpdateDTO.Ean)
        {
            throw new ArgumentException("O código de barras (EAN) não pode ser alterado");
        }

        product.UpdatedAt = DateTime.Now;

        product.ToUpdate(productUpdateDTO);

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return product.ToReadDTO();
    }

    public async Task<ProductReadDTO> DeleteAsync(int id)
    {
        var product = await _context.Products!.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return product.ToReadDTO();
    }

    public async Task<ProductReadDTO> GetBySku(string sku)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Sku == sku);
        if (product == null)
        {
            throw new KeyNotFoundException($"Produto com SKU {sku} não encontrado");
        }
        return product.ToReadDTO();
    }

    public async Task<PagedList<ProductReadDTO>> GetMinStockAsync(ProductParameters productParameters)
    {
        var products = await PagedList<Product>.ToPagedListAsync(
            _context.Products!.Where(p => p.StockQuantity <= p.MinStock).OrderBy(p => p.StockQuantity),
            productParameters.PageNumber,
            productParameters.PageSize
        );
        if (!products.Any())
        {
            throw new KeyNotFoundException("Nenhum produto com estoque mínimo encontrado");
        }

        var productsDto = products.ToReadDTOs().ToList();
        return new PagedList<ProductReadDTO>(productsDto, products.TotalCount, products.CurrentPage, products.PageSize);
    }

    public async Task<ProductReadDTO> UpdateMinStockAsync(int id, int quantity)
    {
        var product = await _context.Products!.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }
        product.MinStock = quantity;
        product.UpdatedAt = DateTime.Now;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product.ToReadDTO();
    }

    public async Task<ProductReadDTO> UpdateActiveAsync(int id, ProductActiveUpdateDTO productUpdateDTO)
    {
        var product = await _context.Products!.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }
        product.IsActive = productUpdateDTO.IsActive;
        product.UpdatedAt = DateTime.Now;
        _context.Products.Update(product);

        await _context.SaveChangesAsync();
        return product.ToReadDTO();
    }
}
