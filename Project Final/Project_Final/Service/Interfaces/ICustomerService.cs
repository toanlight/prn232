using BusinessLayer.Entities.Partners;

namespace Service.Interfaces;

public interface ICustomerService
{
    Task<(List<Customer> Items, int TotalCount)> SearchCustomersAsync(string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10);
    Task<Customer> GetByIdAsync(int id);
    Task<Customer> CreateCustomerAsync(CreateCustomerDto dto);
    Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
}

public record CreateCustomerDto(
    string Code, string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson,
    string CustomerType
);

public record UpdateCustomerDto(
    string Name, string? TaxCode, string? Address,
    string? Email, string? Phone, string? ContactPerson,
    bool IsActive
);
