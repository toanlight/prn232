using BusinessLayer.Entities.Partners;
using BusinessLayer.Enums;

namespace Service.Interfaces;

public interface ISupplierService
{
    Task<(List<Supplier> Items, int TotalCount)> SearchSuppliersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<Supplier> GetByIdAsync(int id);
    Task<Supplier> CreateSupplierAsync(CreateSupplierDto dto, int? userId = null);
    Task<Supplier> UpdateSupplierAsync(int id, UpdateSupplierDto dto, int? userId = null);
    Task ChangeStatusAsync(int id, SupplierStatus status, int? userId = null);
    Task DeleteSupplierAsync(int id, int? userId = null);
}

public record CreateSupplierDto(
    string Code, string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? Website, string? ContactPerson,
    string? ContactPhone, string? ContractNumber, DateTime? ContractStartDate,
    DateTime? ContractEndDate, string? PaymentTerms, string? Notes
);

public record UpdateSupplierDto(
    string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? Website, string? ContactPerson,
    string? ContactPhone, string? ContractNumber, DateTime? ContractStartDate,
    DateTime? ContractEndDate, string? PaymentTerms, string? Notes,
    SupplierStatus Status = SupplierStatus.Active
);
