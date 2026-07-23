using BusinessLayer.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _poRepo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IProductRepository _productRepo;

    public PurchaseOrderService(
        IPurchaseOrderRepository poRepo,
        ISupplierRepository supplierRepo,
        IWarehouseRepository warehouseRepo,
        IProductRepository productRepo)
    {
        _poRepo = poRepo;
        _supplierRepo = supplierRepo;
        _warehouseRepo = warehouseRepo;
        _productRepo = productRepo;
    }

    public async Task<(List<PurchaseOrder> Items, int TotalCount)> SearchAsync(
        string? poNumber, int? supplierId, int? warehouseId, string? status, int pageIndex = 1, int pageSize = 10)
    {
        var query = _poRepo.GetQueryable()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.CreatedByUser)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(poNumber))
            query = query.Where(p => p.PONumber.Contains(poNumber));
        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);
        if (warehouseId.HasValue)
            query = query.Where(p => p.WarehouseId == warehouseId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<PurchaseOrder> GetByIdAsync(int id)
    {
        var po = await _poRepo.GetQueryable()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(p => p.Id == id);

        return po ?? throw new KeyNotFoundException($"Không tìm thấy Đơn mua hàng ID={id}.");
    }

    public async Task<PurchaseOrder> CreateAsync(CreatePurchaseOrderDto dto, int userId)
    {
        if (!await _supplierRepo.ExistsAsync(s => s.Id == dto.SupplierId))
            throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={dto.SupplierId}.");
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");
        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException("Đơn mua hàng phải có ít nhất 1 dòng sản phẩm.");

        var poNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var po = new PurchaseOrder
        {
            PONumber = poNumber,
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedDate = dto.ExpectedDate.HasValue ? DateOnly.FromDateTime(dto.ExpectedDate.Value) : null,
            Notes = dto.Notes,
            Status = "DRAFT",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new POLine
            {
                ProductId = l.ProductId,
                OrderedQty = l.OrderedQty,
                ReceivedQty = 0,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        await _poRepo.AddAsync(po);
        await _poRepo.SaveChangesAsync();
        return po;
    }

    public async Task<PurchaseOrder> UpdateAsync(int id, UpdatePurchaseOrderDto dto, int userId)
    {
        var po = await GetByIdAsync(id);
        if (po.Status != "DRAFT")
            throw new InvalidOperationException("Chỉ được chỉnh sửa đơn mua hàng ở trạng thái DRAFT.");

        po.ExpectedDate = dto.ExpectedDate.HasValue ? DateOnly.FromDateTime(dto.ExpectedDate.Value) : null;
        po.Notes = dto.Notes;
        po.UpdatedAt = DateTime.UtcNow;

        po.Lines.Clear();
        foreach (var line in dto.Lines)
        {
            po.Lines.Add(new POLine
            {
                POId = id,
                ProductId = line.ProductId,
                OrderedQty = line.OrderedQty,
                ReceivedQty = 0,
                UnitPrice = line.UnitPrice
            });
        }

        _poRepo.Update(po);
        await _poRepo.SaveChangesAsync();
        return po;
    }

    public async Task SubmitAsync(int id, int userId)
    {
        var po = await GetByIdAsync(id);
        if (po.Status != "DRAFT")
            throw new InvalidOperationException("Chỉ đơn mua hàng ở trạng thái DRAFT mới có thể Submit.");

        po.Status = "SUBMITTED";
        po.UpdatedAt = DateTime.UtcNow;
        _poRepo.Update(po);
        await _poRepo.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, int userId)
    {
        var po = await GetByIdAsync(id);
        if (po.Status != "DRAFT" && po.Status != "SUBMITTED")
            throw new InvalidOperationException("Chỉ có thể hủy đơn mua hàng ở trạng thái DRAFT hoặc SUBMITTED.");

        po.Status = "CANCELLED";
        po.UpdatedAt = DateTime.UtcNow;
        _poRepo.Update(po);
        await _poRepo.SaveChangesAsync();
    }
}
