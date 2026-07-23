using BusinessLayer.Entities.Partners;
using BusinessLayer.Enums;

namespace Service.Interfaces;

public interface ISupplierService
{
    Task<(List<Supplier> Items, int TotalCount)> SearchSuppliersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<Supplier> GetByIdAsync(int id);
    Task<Supplier> CreateSupplierAsync(CreateSupplierDto dto);
    Task<Supplier> UpdateSupplierAsync(int id, UpdateSupplierDto dto);
    Task ChangeStatusAsync(int id, SupplierStatus status);
}

public record CreateSupplierDto(
    string Code, string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson, string? ContractNumber
);

public record UpdateSupplierDto(
    string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson, string? ContractNumber
);
