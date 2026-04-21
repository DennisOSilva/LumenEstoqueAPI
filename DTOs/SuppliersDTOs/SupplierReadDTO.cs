using LumenEstoque.Validations;
using System.ComponentModel.DataAnnotations;

namespace LumenEstoque.DTOs.SuppliersDTOs;

public class SupplierReadDTO
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Cnpj { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public string? Url { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
