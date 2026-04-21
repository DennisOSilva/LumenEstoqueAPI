using LumenEstoque.DTOs.SuppliersDTOs;
using LumenEstoque.Models;

namespace LumenEstoque.DTOs.Mapping;

public static class SupplierDTOMappingExtentions
{
    public static Supplier ToEntity(this SupplierCreateDTO dto)
    {
        return new Supplier
        {
            Name = dto.Name,
            Cnpj = dto.Cnpj,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            Url = dto.Url,
            IsActive = dto.IsActive
        };
    }

    public static void ToUpdate(this Supplier supplier, SupplierUpdateDTO dto)
    {
        supplier.Name = dto.Name;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.Url = dto.Url;
        supplier.IsActive = dto.IsActive;
    }

    public static SupplierReadDTO ToReadDTO(this Supplier supplier)
    {
        return new SupplierReadDTO
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Cnpj = supplier.Cnpj,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            Url = supplier.Url,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }

    public static IEnumerable<SupplierReadDTO> ToReadDTOs(this IEnumerable<Supplier> suppliers)
    {
        if (suppliers == null || !suppliers.Any())
        {
            return Enumerable.Empty<SupplierReadDTO>();
        }

        return suppliers.Select(p => p.ToReadDTO());
    }
}
