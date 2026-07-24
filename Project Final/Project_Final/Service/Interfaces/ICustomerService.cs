using BusinessLayer.Entities.Partners;

namespace Service.Interfaces;

public interface ICustomerService
{
    Task<(List<Customer> Items, int TotalCount)> SearchCustomersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<Customer> GetByIdAsync(int id);
    Task<Customer> CreateCustomerAsync(CreateCustomerDto dto, int? userId = null);
    Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerDto dto, int? userId = null);
    Task ChangeStatusAsync(int id, bool isActive, int? userId = null);
    Task DeleteCustomerAsync(int id, int? userId = null);
}

public record CreateCustomerDto(
    string Code, string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson, string? ContactPhone,
    string? ContractNumber, DateTime? ContractStartDate, DateTime? ContractEndDate,
    string? ServiceTerms, string? Notes, string CustomerType
);

public record UpdateCustomerDto(
    string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson, string? ContactPhone,
    string? ContractNumber, DateTime? ContractStartDate, DateTime? ContractEndDate,
    string? ServiceTerms, string? Notes, string CustomerType, bool IsActive = true
);
