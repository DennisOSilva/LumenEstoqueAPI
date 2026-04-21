using LumenEstoque.Validations;
using System.ComponentModel.DataAnnotations;

namespace LumenEstoque.DTOs.SuppliersDTOs;

public class SupplierCreateDTO
{
    [Required(ErrorMessage = "O Nome é obrigatório")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O Nome deve ter entre 3 e 50 caracteres")]
    public string? Name { get; set; }

    [Cnpj]
    public string? Cnpj { get; set; }

    [Phone(ErrorMessage = "Telefone inválido")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "O Telefone deve ter entre 10 e 20 caracteres")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(100, ErrorMessage = "O E-mail pode ter no máximo 100 caracteres")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "O Endereço pode ter no máximo 200 caracteres")]
    public string? Address { get; set; }

    [Url(ErrorMessage = "URL inválida.")]
    public string? Url { get; set; }

    public bool IsActive { get; set; } = true;
}
