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

    public async Task<Supplier> CreateSupplierAsync(CreateSupplierDto dto)
    {
        if (await _supplierRepo.ExistsAsync(s => s.Code.ToLower() == dto.Code.ToLower()))
            throw new InvalidOperationException($"Mã NCC '{dto.Code}' đã tồn tại.");

        var supplier = new Supplier
        {
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            TaxCode = dto.TaxCode,
            Address = dto.Address,
            Email = dto.Email,
            Phone = dto.Phone,
            ContactPerson = dto.ContactPerson,
            ContractNumber = dto.ContractNumber,
            Status = SupplierStatus.Active,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _supplierRepo.AddAsync(supplier);
        await _supplierRepo.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier> UpdateSupplierAsync(int id, UpdateSupplierDto dto)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={id}.");

        supplier.Name = dto.Name;
        supplier.TaxCode = dto.TaxCode;
        supplier.Address = dto.Address;
        supplier.Email = dto.Email;
        supplier.Phone = dto.Phone;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.ContractNumber = dto.ContractNumber;
        supplier.UpdatedAt = DateTime.UtcNow;

        _supplierRepo.Update(supplier);
        await _supplierRepo.SaveChangesAsync();
        return supplier;
    }

    public async Task ChangeStatusAsync(int id, SupplierStatus status)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={id}.");
        supplier.Status = status;
        supplier.IsActive = status == SupplierStatus.Active;
        supplier.UpdatedAt = DateTime.UtcNow;
        _supplierRepo.Update(supplier);
        await _supplierRepo.SaveChangesAsync();
    }
}
