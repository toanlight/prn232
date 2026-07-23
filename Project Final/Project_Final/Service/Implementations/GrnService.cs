using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class GrnService : IGrnService
{
    private readonly IGoodsReceiptNoteRepository _grnRepo;
    private readonly IBatchRepository _batchRepo;
    private readonly IBinStockRepository _binStockRepo;
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IPurchaseOrderRepository _poRepo;
    private readonly IApprovalService _approvalService;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IWarehouseRepository _warehouseRepo;

    public GrnService(
        IGoodsReceiptNoteRepository grnRepo,
        IBatchRepository batchRepo,
        IBinStockRepository binStockRepo,
        IStockTransactionRepository txnRepo,
        IPurchaseOrderRepository poRepo,
        IApprovalService approvalService,
        ISupplierRepository supplierRepo,
        IWarehouseRepository warehouseRepo)
    {
        _grnRepo = grnRepo;
        _batchRepo = batchRepo;
        _binStockRepo = binStockRepo;
        _txnRepo = txnRepo;
        _poRepo = poRepo;
        _approvalService = approvalService;
        _supplierRepo = supplierRepo;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<(List<GoodsReceiptNote> Items, int TotalCount)> SearchAsync(
        string? status, int? supplierId, int? warehouseId, DateTime? fromDate, DateTime? toDate, int pageIndex = 1, int pageSize = 10)
    {
        var query = _grnRepo.GetQueryable()
            .Include(g => g.Supplier)
            .Include(g => g.Warehouse)
            .Include(g => g.CreatedByUser)
            .AsNoTracking();

        if (Enum.TryParse<DocumentStatus>(status, true, out var docStatus))
            query = query.Where(g => g.Status == docStatus);
        if (supplierId.HasValue)
            query = query.Where(g => g.SupplierId == supplierId.Value);
        if (warehouseId.HasValue)
            query = query.Where(g => g.WarehouseId == warehouseId.Value);
        if (fromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(fromDate.Value);
            query = query.Where(g => g.ReceiptDate >= from);
        }
        if (toDate.HasValue)
        {
            var to = DateOnly.FromDateTime(toDate.Value);
            query = query.Where(g => g.ReceiptDate <= to);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(g => g.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<GoodsReceiptNote> GetByIdAsync(int id)
    {
        var grn = await _grnRepo.GetQueryable()
            .Include(g => g.Supplier)
            .Include(g => g.Warehouse)
            .Include(g => g.CreatedByUser)
            .Include(g => g.Lines)
                .ThenInclude(l => l.Product)
            .Include(g => g.Lines)
                .ThenInclude(l => l.Bin)
            .FirstOrDefaultAsync(g => g.Id == id);

        return grn ?? throw new KeyNotFoundException($"Không tìm thấy phiếu nhập kho ID={id}.");
    }

    public async Task<GoodsReceiptNote> CreateAsync(CreateGrnDto dto, int userId)
    {
        if (!await _supplierRepo.ExistsAsync(s => s.Id == dto.SupplierId))
            throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp ID={dto.SupplierId}.");
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");
        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException("Phiếu nhập kho phải có ít nhất 1 dòng hàng.");

        var grnNumber = $"GRN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var grn = new GoodsReceiptNote
        {
            GRNNumber = grnNumber,
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            POId = dto.PoId,
            ReceiptDate = DateOnly.FromDateTime(dto.ReceiptDate),
            Notes = dto.Notes,
            Status = DocumentStatus.Draft,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new GRNLine
            {
                ProductId = l.ProductId,
                BinId = l.BinId,
                Quantity = l.Quantity,
                LotNumber = l.LotNumber,
                MfgDate = l.MfgDate,
                ExpiryDate = l.ExpiryDate,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        await _grnRepo.AddAsync(grn);
        await _grnRepo.SaveChangesAsync();
        return grn;
    }

    public async Task<GoodsReceiptNote> UpdateAsync(int id, UpdateGrnDto dto, int userId)
    {
        var grn = await GetByIdAsync(id);
        if (grn.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Chỉ được sửa phiếu nhập kho ở trạng thái DRAFT.");

        grn.ReceiptDate = DateOnly.FromDateTime(dto.ReceiptDate);
        grn.Notes = dto.Notes;
        grn.UpdatedAt = DateTime.UtcNow;

        grn.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            grn.Lines.Add(new GRNLine
            {
                GRNId = id,
                ProductId = l.ProductId,
                BinId = l.BinId,
                Quantity = l.Quantity,
                LotNumber = l.LotNumber,
                MfgDate = l.MfgDate,
                ExpiryDate = l.ExpiryDate,
                UnitPrice = l.UnitPrice
            });
        }

        _grnRepo.Update(grn);
        await _grnRepo.SaveChangesAsync();
        return grn;
    }

    public async Task SubmitAsync(int id, int userId)
    {
        var grn = await GetByIdAsync(id);
        if (grn.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Chỉ phiếu ở trạng thái DRAFT mới có thể Submit.");

        grn.Status = DocumentStatus.PendingL1;
        grn.UpdatedAt = DateTime.UtcNow;
        _grnRepo.Update(grn);
        await _grnRepo.SaveChangesAsync();

        // Tạo yêu cầu duyệt L1
        await _approvalService.CreateApprovalRequestAsync(DocumentType.Grn, grn.Id, userId);
    }

    public async Task CancelAsync(int id, int userId)
    {
        var grn = await GetByIdAsync(id);
        if (grn.Status != DocumentStatus.Draft && grn.Status != DocumentStatus.Rejected)
            throw new InvalidOperationException("Chỉ có thể hủy phiếu ở trạng thái DRAFT hoặc REJECTED.");

        grn.Status = DocumentStatus.Cancelled;
        grn.UpdatedAt = DateTime.UtcNow;
        _grnRepo.Update(grn);
        await _grnRepo.SaveChangesAsync();
    }

    /// <summary>
    /// Được kích hoạt tự động sau khi Approval Engine duyệt L2 thành công.
    /// Thực hiện tạo/tìm Batch, tăng BinStock, và ghi StockTransaction (GRN_IN).
    /// </summary>
    public async Task FinalizeAsync(int grnId)
    {
        var grn = await GetByIdAsync(grnId);
        if (grn.Status == DocumentStatus.Approved)
            return; // Đã finalize rồi

        grn.Status = DocumentStatus.Approved;
        grn.CompletedAt = DateTime.UtcNow;
        grn.UpdatedAt = DateTime.UtcNow;

        foreach (var line in grn.Lines)
        {
            var lotNo = string.IsNullOrWhiteSpace(line.LotNumber) ? $"LOT-{grn.GRNNumber}" : line.LotNumber;

            // 1. Tìm hoặc tạo Batch
            var batch = await _batchRepo.FindAsync(b =>
                b.ProductId == line.ProductId &&
                b.LotNumber == lotNo &&
                b.SupplierId == grn.SupplierId);

            if (batch == null)
            {
                batch = new Batch
                {
                    ProductId = line.ProductId,
                    SupplierId = grn.SupplierId,
                    LotNumber = lotNo,
                    MfgDate = line.MfgDate,
                    ExpiryDate = line.ExpiryDate,
                    InitialQty = line.Quantity,
                    Status = BatchStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = grn.CreatedBy
                };
                await _batchRepo.AddAsync(batch);
                await _batchRepo.SaveChangesAsync();
            }

            line.BatchId = batch.Id;

            // 2. Upsert BinStock
            var binStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.BinId &&
                bs.BatchId == batch.Id &&
                bs.ProductId == line.ProductId);

            decimal qtyBefore = binStock?.Quantity ?? 0;

            if (binStock == null)
            {
                binStock = new BinStock
                {
                    BinId = line.BinId,
                    BatchId = batch.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    ReservedQty = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _binStockRepo.AddAsync(binStock);
            }
            else
            {
                binStock.Quantity += line.Quantity;
                binStock.UpdatedAt = DateTime.UtcNow;
                _binStockRepo.Update(binStock);
            }

            decimal qtyAfter = binStock.Quantity;

            // 3. Ghi StockTransaction
            var txn = new StockTransaction
            {
                ProductId = line.ProductId,
                BatchId = batch.Id,
                BinId = line.BinId,
                TxnType = StockTxnType.GrnIn,
                DocumentType = DocumentType.Grn,
                DocumentId = grn.Id,
                DocumentNumber = grn.GRNNumber,
                Quantity = line.Quantity,
                QtyBefore = qtyBefore,
                QtyAfter = qtyAfter,
                Remarks = $"Nhập kho theo phiếu {grn.GRNNumber}",
                CreatedBy = grn.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _txnRepo.AddAsync(txn);

            // 4. Nếu có POId -> Cập nhật ReceivedQty trong POLine
            if (grn.POId.HasValue)
            {
                var po = await _poRepo.GetQueryable()
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == grn.POId.Value);

                if (po != null)
                {
                    var poLine = po.Lines.FirstOrDefault(l => l.ProductId == line.ProductId);
                    if (poLine != null)
                    {
                        poLine.ReceivedQty += line.Quantity;
                    }

                    // Nếu tất cả các line đã nhận đủ -> Đổi PO sang COMPLETED
                    if (po.Lines.All(l => l.ReceivedQty >= l.OrderedQty))
                        po.Status = "COMPLETED";

                    _poRepo.Update(po);
                }
            }
        }

        _grnRepo.Update(grn);
        await _grnRepo.SaveChangesAsync();
    }
}
