using LumenEstoque.Context;
using LumenEstoque.DTOs.CategoriesDTOs;
using LumenEstoque.DTOs.Mapping;
using LumenEstoque.Models;
using LumenEstoque.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LumenEstoque.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<CategoryReadDTO>> GetAllAsync(CategoryParameters categoryParameters)
    {
        var categories = await PagedList<Category>.ToPagedListAsync(
            _context.Categories.AsQueryable(),
            categoryParameters.PageNumber,
            categoryParameters.PageSize);

        if (!categories.Any())
        {
            return new PagedList<CategoryReadDTO>(new List<CategoryReadDTO>(), 0, categoryParameters.PageNumber, categoryParameters.PageSize);
        }

        return new PagedList<CategoryReadDTO>(categories.Select(c => c.ToReadDTO()).ToList(), categories.TotalCount, categories.CurrentPage, categories.PageSize);
    }

    public async Task<CategoryReadDTO?> GetByIdAsync(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if(category == null)
        {
            throw new KeyNotFoundException($"Categoria com ID {id} não encontrada");
        }

        return category.ToReadDTO();
    }

    public async Task<CategoryReadDTO> CreateAsync(CategoryCreateDTO categoryCreateDTO)
    {
        var category = categoryCreateDTO.ToEntity();

        if(category == null)
        {
            throw new ArgumentException("Erro ao criar a categoria");
        }

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return category.ToReadDTO();
    }

    public async Task<CategoryReadDTO> UpdateAsync(int id, CategoryUpdateDTO categoryUpdateDTO)
    {
        var category = _context.Categories.FirstOrDefault(c => c.Id == id);

        if(category == null)
        {
            throw new KeyNotFoundException($"Categoria com ID {id} não encontrada");
        }

        category.ToUpdate(categoryUpdateDTO);

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return category.ToReadDTO();
    }

    public async Task<CategoryReadDTO> DeleteAsync(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if(category == null)
        {
            throw new KeyNotFoundException($"Categoria com ID {id} não encontrada");
        }

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

        if(hasProducts == true)
        {
            throw new ArgumentException($"Não é possível excluir a categoria com ID {id} porque existem produtos associados a ela");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return category.ToReadDTO();
    }
}
