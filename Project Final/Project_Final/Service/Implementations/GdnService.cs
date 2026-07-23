using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class GdnService : IGdnService
{
    private readonly IGoodsDispatchNoteRepository _gdnRepo;
    private readonly IBinStockRepository _binStockRepo;
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IApprovalService _approvalService;
    private readonly ICustomerRepository _customerRepo;
    private readonly IWarehouseRepository _warehouseRepo;

    public GdnService(
        IGoodsDispatchNoteRepository gdnRepo,
        IBinStockRepository binStockRepo,
        IStockTransactionRepository txnRepo,
        IApprovalService approvalService,
        ICustomerRepository customerRepo,
        IWarehouseRepository warehouseRepo)
    {
        _gdnRepo = gdnRepo;
        _binStockRepo = binStockRepo;
        _txnRepo = txnRepo;
        _approvalService = approvalService;
        _customerRepo = customerRepo;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<(List<GoodsDispatchNote> Items, int TotalCount)> SearchAsync(
        string? status, int? customerId, int? warehouseId, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10)
    {
        var query = _gdnRepo.GetQueryable()
            .Include(g => g.Customer)
            .Include(g => g.Warehouse)
            .Include(g => g.CreatedByUser)
            .AsNoTracking();

        if (Enum.TryParse<DocumentStatus>(status, true, out var docStatus))
            query = query.Where(g => g.Status == docStatus);
        if (customerId.HasValue)
            query = query.Where(g => g.CustomerId == customerId.Value);
        if (warehouseId.HasValue)
            query = query.Where(g => g.WarehouseId == warehouseId.Value);
        if (fromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(fromDate.Value);
            query = query.Where(g => g.DispatchDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateOnly.FromDateTime(toDate.Value);
            query = query.Where(g => g.DispatchDate <= to);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(g => g.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<GoodsDispatchNote> GetByIdAsync(int id)
    {
        var gdn = await _gdnRepo.GetQueryable()
            .Include(g => g.Customer)
            .Include(g => g.Warehouse)
            .Include(g => g.CreatedByUser)
            .Include(g => g.PickedByUser)
            .Include(g => g.DeliveredByUser)
            .Include(g => g.Lines)
                .ThenInclude(l => l.Product)
            .Include(g => g.Lines)
                .ThenInclude(l => l.Batch)
            .Include(g => g.Lines)
                .ThenInclude(l => l.Bin)
            .FirstOrDefaultAsync(g => g.Id == id);

        return gdn ?? throw new KeyNotFoundException($"Không tìm thấy phiếu xuất kho ID={id}.");
    }

    public async Task<GoodsDispatchNote> CreateAsync(CreateGdnDto dto, int userId)
    {
        if (!await _customerRepo.ExistsAsync(c => c.Id == dto.CustomerId))
            throw new KeyNotFoundException($"Không tìm thấy khách hàng ID={dto.CustomerId}.");
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");
        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException("Phiếu xuất kho phải có ít nhất 1 dòng hàng.");

        // Kiểm tra tồn kho khả dụng và giữ hàng (Reserve)
        foreach (var line in dto.Lines)
        {
            if (!line.BatchId.HasValue)
                throw new InvalidOperationException("Mỗi dòng xuất kho phải chỉ định BatchId.");

            var binStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.BinId &&
                bs.BatchId == line.BatchId.Value &&
                bs.ProductId == line.ProductId)
                ?? throw new InvalidOperationException($"Không tìm thấy hàng trong vị trí chỉ định (Product={line.ProductId}, Bin={line.BinId}, Batch={line.BatchId}).");

            if (binStock.Quantity - binStock.ReservedQty < line.Quantity)
                throw new InvalidOperationException($"Không đủ tồn kho khả dụng tại Bin {line.BinId} cho sản phẩm {line.ProductId}. (Khả dụng: {binStock.Quantity - binStock.ReservedQty}, Yêu cầu: {line.Quantity})");

            // Đặt giữ chỗ (Reserve)
            binStock.ReservedQty += line.Quantity;
            binStock.UpdatedAt = DateTime.UtcNow;
            _binStockRepo.Update(binStock);
        }

        var gdnNumber = $"GDN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var gdn = new GoodsDispatchNote
        {
            GDNNumber = gdnNumber,
            CustomerId = dto.CustomerId,
            WarehouseId = dto.WarehouseId,
            RequestId = dto.DispatchRequestId,
            DispatchDate = dto.DispatchDate.HasValue ? DateOnly.FromDateTime(dto.DispatchDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = dto.Notes,
            Status = DocumentStatus.Draft,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new GDNLine
            {
                ProductId = l.ProductId,
                BinId = l.BinId,
                BatchId = l.BatchId!.Value,
                RequestedQty = l.Quantity,
                PickedQty = 0
            }).ToList()
        };

        await _gdnRepo.AddAsync(gdn);
        await _gdnRepo.SaveChangesAsync();
        await _binStockRepo.SaveChangesAsync();
        return gdn;
    }

    public async Task<GoodsDispatchNote> UpdateAsync(int id, UpdateGdnDto dto, int userId)
    {
        var gdn = await GetByIdAsync(id);
        if (gdn.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Chỉ được chỉnh sửa phiếu xuất kho ở trạng thái DRAFT.");

        gdn.DispatchDate = dto.DispatchDate.HasValue ? DateOnly.FromDateTime(dto.DispatchDate.Value) : gdn.DispatchDate;
        gdn.Notes = dto.Notes;
        gdn.UpdatedAt = DateTime.UtcNow;

        _gdnRepo.Update(gdn);
        await _gdnRepo.SaveChangesAsync();
        return gdn;
    }

    public async Task SubmitAsync(int id, int userId)
    {
        var gdn = await GetByIdAsync(id);
        if (gdn.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Chỉ phiếu DRAFT mới có thể Submit.");

        gdn.Status = DocumentStatus.PendingL1;
        gdn.UpdatedAt = DateTime.UtcNow;
        _gdnRepo.Update(gdn);
        await _gdnRepo.SaveChangesAsync();

        await _approvalService.CreateApprovalRequestAsync(DocumentType.Gdn, gdn.Id, userId);
    }

    public async Task CancelAsync(int id, int userId)
    {
        var gdn = await GetByIdAsync(id);
        if (gdn.Status == DocumentStatus.Delivered || gdn.Status == DocumentStatus.Cancelled)
            throw new InvalidOperationException("Không thể hủy phiếu đã hoàn thành hoặc đã bị hủy.");

        // Giải phóng ReservedQty trên BinStock
        foreach (var line in gdn.Lines)
        {
            var binStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.BinId &&
                bs.BatchId == line.BatchId &&
                bs.ProductId == line.ProductId);

            if (binStock != null)
            {
                binStock.ReservedQty = Math.Max(0, binStock.ReservedQty - line.RequestedQty);
                binStock.UpdatedAt = DateTime.UtcNow;
                _binStockRepo.Update(binStock);
            }
        }

        gdn.Status = DocumentStatus.Cancelled;
        gdn.UpdatedAt = DateTime.UtcNow;
        _gdnRepo.Update(gdn);

        await _gdnRepo.SaveChangesAsync();
        await _binStockRepo.SaveChangesAsync();
    }

    public async Task FinalizeAsync(int gdnId)
    {
        var gdn = await GetByIdAsync(gdnId);
        if (gdn.Status == DocumentStatus.Approved) return;

        gdn.Status = DocumentStatus.Approved;
        gdn.UpdatedAt = DateTime.UtcNow;
        _gdnRepo.Update(gdn);
        await _gdnRepo.SaveChangesAsync();
    }

    public async Task StartPickingAsync(int id, int userId)
    {
        var gdn = await GetByIdAsync(id);
        if (gdn.Status != DocumentStatus.Approved)
            throw new InvalidOperationException("Chỉ phiếu đã được APPROVED mới có thể bắt đầu lấy hàng (Picking).");

        gdn.Status = DocumentStatus.Picking;
        gdn.PickedBy = userId;
        gdn.UpdatedAt = DateTime.UtcNow;
        _gdnRepo.Update(gdn);
        await _gdnRepo.SaveChangesAsync();
    }

    public async Task ConfirmPickedAsync(int id, int userId)
    {
        var gdn = await GetByIdAsync(id);
        if (gdn.Status != DocumentStatus.Picking)
            throw new InvalidOperationException("Chỉ phiếu đang PICKING mới có thể xác nhận lấy hàng.");

        foreach (var line in gdn.Lines)
        {
            line.PickedQty = line.RequestedQty; // Mặc định lấy đủ
        }

        gdn.Status = DocumentStatus.Picked;
        gdn.PickedAt = DateTime.UtcNow;
        gdn.UpdatedAt = DateTime.UtcNow;
        _gdnRepo.Update(gdn);
        await _gdnRepo.SaveChangesAsync();
    }

    public async Task DeliverAsync(int id, int userId, DeliverGdnDto dto)
    {
        var gdn = await GetByIdAsync(id);
        if (gdn.Status != DocumentStatus.Picked && gdn.Status != DocumentStatus.Picking)
            throw new InvalidOperationException("Chỉ phiếu đã Picked mới có thể xác nhận Giao hàng.");

        // Cập nhật số lượng lấy hàng thực tế nếu có
        if (dto.ActualLines != null && dto.ActualLines.Count > 0)
        {
            foreach (var actual in dto.ActualLines)
            {
                var line = gdn.Lines.FirstOrDefault(l => l.Id == actual.GdnLineId);
                if (line != null)
                    line.PickedQty = actual.PickedQty;
            }
        }

        // Thực hiện trừ tồn kho thực tế & giải phóng ReservedQty & ghi StockTransaction
        foreach (var line in gdn.Lines)
        {
            var actualQty = line.PickedQty ?? line.RequestedQty;

            var binStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.BinId &&
                bs.BatchId == line.BatchId &&
                bs.ProductId == line.ProductId)
                ?? throw new InvalidOperationException($"Không tìm thấy BinStock để trừ kho (Bin={line.BinId}, Batch={line.BatchId}, Product={line.ProductId}).");

            decimal qtyBefore = binStock.Quantity;
            binStock.Quantity = Math.Max(0, binStock.Quantity - actualQty);
            binStock.ReservedQty = Math.Max(0, binStock.ReservedQty - line.RequestedQty);
            binStock.UpdatedAt = DateTime.UtcNow;
            _binStockRepo.Update(binStock);

            decimal qtyAfter = binStock.Quantity;

            // Ghi StockTransaction GDN_OUT
            var txn = new StockTransaction
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                BinId = line.BinId,
                TxnType = StockTxnType.GdnOut,
                DocumentType = DocumentType.Gdn,
                DocumentId = gdn.Id,
                DocumentNumber = gdn.GDNNumber,
                Quantity = actualQty,
                QtyBefore = qtyBefore,
                QtyAfter = qtyAfter,
                Remarks = $"Xuất kho theo phiếu {gdn.GDNNumber} cho KH ID={gdn.CustomerId}",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _txnRepo.AddAsync(txn);
        }

        gdn.Status = DocumentStatus.Delivered;
        gdn.DeliveredBy = userId;
        gdn.DeliveredAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.DeliveryNote))
            gdn.Notes = (gdn.Notes != null ? gdn.Notes + " | " : "") + dto.DeliveryNote;

        gdn.UpdatedAt = DateTime.UtcNow;

        _gdnRepo.Update(gdn);
        await _gdnRepo.SaveChangesAsync();
        await _binStockRepo.SaveChangesAsync();
    }
}
