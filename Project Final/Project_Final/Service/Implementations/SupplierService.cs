using System.Text.RegularExpressions;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Enums;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepo;

    public SupplierService(ISupplierRepository supplierRepo)
        => _supplierRepo = supplierRepo;

    public async Task<(List<Supplier> Items, int TotalCount)> SearchSuppliersAsync(
        string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _supplierRepo.SearchAsync(keyword, isActive, pageIndex, pageSize);

    public async Task<Supplier> GetByIdAsync(int id)
        => await _supplierRepo.GetByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={id}.");

    public async Task<Supplier> CreateSupplierAsync(CreateSupplierDto dto, int? userId = null)
    {
        await ValidateSupplierAsync(
            dto.Code, dto.Name, dto.Phone, dto.ContactPhone, dto.TaxCode, dto.Email, dto.Website,
            dto.ContractStartDate, dto.ContractEndDate);

        var supplier = new Supplier
        {
            Code = dto.Code.Trim().ToUpper(),
            Name = dto.Name.Trim(),
            TaxCode = dto.TaxCode?.Trim(),
            Address = dto.Address?.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            Website = dto.Website?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            ContractNumber = dto.ContractNumber?.Trim(),
            ContractStartDate = dto.ContractStartDate,
            ContractEndDate = dto.ContractEndDate,
            PaymentTerms = dto.PaymentTerms?.Trim(),
            Notes = dto.Notes?.Trim(),
            Status = SupplierStatus.Active,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _supplierRepo.AddAsync(supplier);
        await _supplierRepo.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier> UpdateSupplierAsync(int id, UpdateSupplierDto dto, int? userId = null)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={id}.");

        await ValidateSupplierAsync(
            supplier.Code, dto.Name, dto.Phone, dto.ContactPhone, dto.TaxCode, dto.Email, dto.Website,
            dto.ContractStartDate, dto.ContractEndDate, currentSupplierId: id);

        supplier.Name = dto.Name.Trim();
        supplier.TaxCode = dto.TaxCode?.Trim();
        supplier.Address = dto.Address?.Trim();
        supplier.Email = dto.Email?.Trim();
        supplier.Phone = dto.Phone?.Trim();
        supplier.Website = dto.Website?.Trim();
        supplier.ContactPerson = dto.ContactPerson?.Trim();
        supplier.ContactPhone = dto.ContactPhone?.Trim();
        supplier.ContractNumber = dto.ContractNumber?.Trim();
        supplier.ContractStartDate = dto.ContractStartDate;
        supplier.ContractEndDate = dto.ContractEndDate;
        supplier.PaymentTerms = dto.PaymentTerms?.Trim();
        supplier.Notes = dto.Notes?.Trim();
        supplier.Status = dto.Status;
        supplier.IsActive = dto.Status == SupplierStatus.Active;
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = userId;

        _supplierRepo.Update(supplier);
        await _supplierRepo.SaveChangesAsync();
        return supplier;
    }

    public async Task ChangeStatusAsync(int id, SupplierStatus status, int? userId = null)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={id}.");
        supplier.Status = status;
        supplier.IsActive = status == SupplierStatus.Active;
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = userId;
        _supplierRepo.Update(supplier);
        await _supplierRepo.SaveChangesAsync();
    }

    public async Task DeleteSupplierAsync(int id, int? userId = null)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={id}.");

        var hasSuppliedGoods = await _supplierRepo.HasSuppliedGoodsAsync(id);
        if (hasSuppliedGoods)
        {
            throw new InvalidOperationException("Không thể xóa nhà cung cấp này vì đã có hàng hóa, đơn mua hàng (PO), phiếu nhập kho (GRN) hoặc lô hàng liên quan.");
        }

        _supplierRepo.Remove(supplier);
        await _supplierRepo.SaveChangesAsync();
    }

    private async Task ValidateSupplierAsync(
        string code,
        string name,
        string? phone,
        string? contactPhone,
        string? taxCode,
        string? email,
        string? website,
        DateTime? contractStartDate,
        DateTime? contractEndDate,
        int? currentSupplierId = null)
    {
        // 1. Code validation: Required, Unique, 3-20 chars
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Mã nhà cung cấp là bắt buộc.");
        var trimmedCode = code.Trim();
        if (trimmedCode.Length < 3 || trimmedCode.Length > 20)
            throw new ArgumentException("Mã nhà cung cấp phải từ 3 đến 20 ký tự.");

        if (currentSupplierId == null)
        {
            if (await _supplierRepo.ExistsAsync(s => s.Code.ToLower() == trimmedCode.ToLower()))
                throw new InvalidOperationException($"Mã nhà cung cấp '{trimmedCode}' đã tồn tại.");
        }
        else
        {
            if (await _supplierRepo.ExistsAsync(s => s.Id != currentSupplierId.Value && s.Code.ToLower() == trimmedCode.ToLower()))
                throw new InvalidOperationException($"Mã nhà cung cấp '{trimmedCode}' đã tồn tại.");
        }

        // 2. Name validation: Required, 3-255 chars
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà cung cấp là bắt buộc.");
        var trimmedName = name.Trim();
        if (trimmedName.Length < 3 || trimmedName.Length > 255)
            throw new ArgumentException("Tên nhà cung cấp phải từ 3 đến 255 ký tự.");

        // 3. Phone validation: Required, Valid phone number format
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Số điện thoại nhà cung cấp là bắt buộc.");
        var trimmedPhone = phone.Trim();
        if (!Regex.IsMatch(trimmedPhone, @"^(\+?84|0)[0-9\s\.\-]{8,15}$") && !Regex.IsMatch(trimmedPhone, @"^[0-9\+\-\.\s\(\)]{8,20}$"))
            throw new ArgumentException("Số điện thoại không đúng định dạng hợp lệ.");

        // 3b. ContactPhone validation: Optional, if provided must be valid phone format
        if (!string.IsNullOrWhiteSpace(contactPhone))
        {
            var trimmedContactPhone = contactPhone.Trim();
            if (!Regex.IsMatch(trimmedContactPhone, @"^(\+?84|0)[0-9\s\.\-]{8,15}$") && !Regex.IsMatch(trimmedContactPhone, @"^[0-9\+\-\.\s\(\)]{8,20}$"))
                throw new ArgumentException("Số điện thoại người liên hệ không đúng định dạng hợp lệ.");
        }

        // 4. TaxCode validation: Optional, Unique, Valid format
        if (!string.IsNullOrWhiteSpace(taxCode))
        {
            var trimmedTax = taxCode.Trim();
            if (!Regex.IsMatch(trimmedTax, @"^[0-9]{10}(-[0-9]{3})?$") && !Regex.IsMatch(trimmedTax, @"^[A-Za-z0-9\-]{8,15}$"))
                throw new ArgumentException("Mã số thuế không đúng định dạng hợp lệ (VD: 0101234567 hoặc 0101234567-001).");

            if (currentSupplierId == null)
            {
                if (await _supplierRepo.ExistsAsync(s => s.TaxCode != null && s.TaxCode.ToLower() == trimmedTax.ToLower()))
                    throw new InvalidOperationException($"Mã số thuế '{trimmedTax}' đã tồn tại.");
            }
            else
            {
                if (await _supplierRepo.ExistsAsync(s => s.Id != currentSupplierId.Value && s.TaxCode != null && s.TaxCode.ToLower() == trimmedTax.ToLower()))
                    throw new InvalidOperationException($"Mã số thuế '{trimmedTax}' đã tồn tại ở nhà cung cấp khác.");
            }
        }

        // 5. Email validation: Optional, Valid format
        if (!string.IsNullOrWhiteSpace(email))
        {
            var trimmedEmail = email.Trim();
            if (!Regex.IsMatch(trimmedEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Email không đúng định dạng hợp lệ.");
        }

        // 6. Website validation: Optional, Valid URL
        if (!string.IsNullOrWhiteSpace(website))
        {
            var trimmedWeb = website.Trim();
            bool isValidUrl = Uri.TryCreate(trimmedWeb, UriKind.Absolute, out var uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!isValidUrl)
            {
                isValidUrl = Uri.TryCreate("http://" + trimmedWeb, UriKind.Absolute, out var uriResult2)
                             && (uriResult2.Scheme == Uri.UriSchemeHttp || uriResult2.Scheme == Uri.UriSchemeHttps);
            }
            if (!isValidUrl)
                throw new ArgumentException("Website không phải là đường dẫn URL hợp lệ.");
        }

        // 7. ContractStartDate & ContractEndDate validation
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
    }
}
