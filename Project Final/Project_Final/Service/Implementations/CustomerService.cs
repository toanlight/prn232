using System.Text.RegularExpressions;
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

    public async Task<Customer> CreateCustomerAsync(CreateCustomerDto dto, int? userId = null)
    {
        var (customerType, _) = await ValidateCustomerAsync(
            dto.Code, dto.Name, dto.CustomerType, dto.Phone, dto.ContactPhone, dto.TaxCode, dto.Email,
            dto.ContractStartDate, dto.ContractEndDate);

        var customer = new Customer
        {
            Code = dto.Code.Trim().ToUpper(),
            Name = dto.Name.Trim(),
            CustomerType = customerType,
            TaxCode = dto.TaxCode?.Trim(),
            Address = dto.Address?.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            ContractNumber = dto.ContractNumber?.Trim(),
            ContractStartDate = dto.ContractStartDate,
            ContractEndDate = dto.ContractEndDate,
            ServiceTerms = dto.ServiceTerms?.Trim(),
            Notes = dto.Notes?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _customerRepo.AddAsync(customer);
        await _customerRepo.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerDto dto, int? userId = null)
    {
        var customer = await _customerRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng ID={id}.");

        var (customerType, _) = await ValidateCustomerAsync(
            customer.Code, dto.Name, dto.CustomerType, dto.Phone, dto.ContactPhone, dto.TaxCode, dto.Email,
            dto.ContractStartDate, dto.ContractEndDate, currentCustomerId: id);

        customer.Name = dto.Name.Trim();
        customer.CustomerType = customerType;
        customer.TaxCode = dto.TaxCode?.Trim();
        customer.Address = dto.Address?.Trim();
        customer.Email = dto.Email?.Trim();
        customer.Phone = dto.Phone?.Trim();
        customer.ContactPerson = dto.ContactPerson?.Trim();
        customer.ContactPhone = dto.ContactPhone?.Trim();
        customer.ContractNumber = dto.ContractNumber?.Trim();
        customer.ContractStartDate = dto.ContractStartDate;
        customer.ContractEndDate = dto.ContractEndDate;
        customer.ServiceTerms = dto.ServiceTerms?.Trim();
        customer.Notes = dto.Notes?.Trim();
        customer.IsActive = dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = userId;

        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();
        return customer;
    }

    public async Task ChangeStatusAsync(int id, bool isActive, int? userId = null)
    {
        var customer = await _customerRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng ID={id}.");
        customer.IsActive = isActive;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = userId;
        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(int id, int? userId = null)
    {
        var customer = await _customerRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khách hàng ID={id}.");

        var hasDispatchedGoods = await _customerRepo.HasDispatchedGoodsAsync(id);
        if (hasDispatchedGoods)
        {
            throw new InvalidOperationException("Không thể xóa khách hàng này vì đã có đơn xuất hàng (GoodsDispatchNote / DispatchRequest) liên quan.");
        }

        _customerRepo.Remove(customer);
        await _customerRepo.SaveChangesAsync();
    }

    private async Task<(CustomerType Type, bool Valid)> ValidateCustomerAsync(
        string code,
        string name,
        string customerTypeStr,
        string? phone,
        string? contactPhone,
        string? taxCode,
        string? email,
        DateTime? contractStartDate,
        DateTime? contractEndDate,
        int? currentCustomerId = null)
    {
        // 1. Code validation
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Mã khách hàng là bắt buộc.");
        var trimmedCode = code.Trim();
        if (trimmedCode.Length < 3 || trimmedCode.Length > 20)
            throw new ArgumentException("Mã khách hàng phải từ 3 đến 20 ký tự.");

        if (currentCustomerId == null)
        {
            if (await _customerRepo.ExistsAsync(c => c.Code.ToLower() == trimmedCode.ToLower()))
                throw new InvalidOperationException($"Mã khách hàng '{trimmedCode}' đã tồn tại.");
        }
        else
        {
            if (await _customerRepo.ExistsAsync(c => c.Id != currentCustomerId.Value && c.Code.ToLower() == trimmedCode.ToLower()))
                throw new InvalidOperationException($"Mã khách hàng '{trimmedCode}' đã tồn tại.");
        }

        // 2. Name validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên khách hàng là bắt buộc.");
        var trimmedName = name.Trim();
        if (trimmedName.Length < 3 || trimmedName.Length > 255)
            throw new ArgumentException("Tên khách hàng phải từ 3 đến 255 ký tự.");

        // 3. CustomerType validation
        if (!Enum.TryParse<CustomerType>(customerTypeStr, true, out var customerType))
            throw new ArgumentException($"Loại khách hàng '{customerTypeStr}' không hợp lệ.");

        // 4. Phone validation (if provided)
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var trimmedPhone = phone.Trim();
            if (!Regex.IsMatch(trimmedPhone, @"^(\+?84|0)[0-9\s\.\-]{8,15}$") && !Regex.IsMatch(trimmedPhone, @"^[0-9\+\-\.\s\(\)]{8,20}$"))
                throw new ArgumentException("Số điện thoại không đúng định dạng hợp lệ.");
        }

        // 5. ContactPhone validation (if provided)
        if (!string.IsNullOrWhiteSpace(contactPhone))
        {
            var trimmedContactPhone = contactPhone.Trim();
            if (!Regex.IsMatch(trimmedContactPhone, @"^(\+?84|0)[0-9\s\.\-]{8,15}$") && !Regex.IsMatch(trimmedContactPhone, @"^[0-9\+\-\.\s\(\)]{8,20}$"))
                throw new ArgumentException("Số điện thoại người liên hệ không đúng định dạng hợp lệ.");
        }

        // 6. TaxCode validation (optional, unique check)
        if (!string.IsNullOrWhiteSpace(taxCode))
        {
            var trimmedTax = taxCode.Trim();
            if (!Regex.IsMatch(trimmedTax, @"^[0-9]{10}(-[0-9]{3})?$") && !Regex.IsMatch(trimmedTax, @"^[A-Za-z0-9\-]{8,15}$"))
                throw new ArgumentException("Mã số thuế không đúng định dạng hợp lệ (VD: 0101234567 hoặc 0101234567-001).");

            if (currentCustomerId == null)
            {
                if (await _customerRepo.ExistsAsync(c => c.TaxCode != null && c.TaxCode.ToLower() == trimmedTax.ToLower()))
                    throw new InvalidOperationException($"Mã số thuế '{trimmedTax}' đã tồn tại.");
            }
            else
            {
                if (await _customerRepo.ExistsAsync(c => c.Id != currentCustomerId.Value && c.TaxCode != null && c.TaxCode.ToLower() == trimmedTax.ToLower()))
                    throw new InvalidOperationException($"Mã số thuế '{trimmedTax}' đã tồn tại ở khách hàng khác.");
            }
        }

        // 7. Email validation
        if (!string.IsNullOrWhiteSpace(email))
        {
            var trimmedEmail = email.Trim();
            if (!Regex.IsMatch(trimmedEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email không đúng định dạng hợp lệ.");
        }

        // 8. Contract dates validation
        if (contractStartDate.HasValue && contractStartDate.Value.Date > DateTime.Now.Date)
        {
            throw new ArgumentException("Ngày bắt đầu hợp đồng phải nhỏ hơn hoặc bằng ngày hiện tại.");
        }

        if (contractEndDate.HasValue)
        {
            if (!contractStartDate.HasValue)
            {
                throw new ArgumentException("Cần nhập Ngày bắt đầu hợp đồng trước khi chọn Ngày kết thúc hợp đồng.");
            }
            if (contractEndDate.Value.Date <= contractStartDate.Value.Date)
            {
                throw new ArgumentException("Ngày kết thúc hợp đồng phải lớn hơn ngày bắt đầu hợp đồng.");
            }
        }

        return (customerType, true);
    }
}
