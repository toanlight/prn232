using BusinessLayer.Entities.Partners;
using BusinessLayer.Enums;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;

    public CustomerService(ICustomerRepository customerRepo)
        => _customerRepo = customerRepo;

    public async Task<(List<Customer> Items, int TotalCount)> SearchCustomersAsync(
        string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _customerRepo.SearchAsync(keyword, isActive, pageIndex, pageSize);

    public async Task<Customer> GetByIdAsync(int id)
        => await _customerRepo.GetByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng ID={id}.");

    public async Task<Customer> CreateCustomerAsync(CreateCustomerDto dto)
    {
        if (await _customerRepo.ExistsAsync(c => c.Code.ToLower() == dto.Code.ToLower()))
            throw new InvalidOperationException($"Mã khách hàng '{dto.Code}' đã tồn tại.");

        if (!Enum.TryParse<CustomerType>(dto.CustomerType, true, out var customerType))
            throw new ArgumentException($"Loại khách hàng '{dto.CustomerType}' không hợp lệ.");

        var customer = new Customer
        {
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            TaxCode = dto.TaxCode,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone,
            ContactPerson = dto.ContactPerson,
            CustomerType = customerType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _customerRepo.AddAsync(customer);
        await _customerRepo.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng ID={id}.");

        customer.Name = dto.Name;
        customer.TaxCode = dto.TaxCode;
        customer.Address = dto.Address;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.ContactPerson = dto.ContactPerson;
        customer.IsActive = dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();
        return customer;
    }
}
