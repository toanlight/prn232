using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IStockAdjustmentRepository _adjRepo;
    private readonly IBinStockRepository _binStockRepo;
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IApprovalService _approvalService;
    private readonly IWarehouseRepository _warehouseRepo;

    public StockAdjustmentService(
        IStockAdjustmentRepository adjRepo,
        IBinStockRepository binStockRepo,
        IStockTransactionRepository txnRepo,
        IApprovalService approvalService,
        IWarehouseRepository warehouseRepo)
    {
        _adjRepo = adjRepo;
        _binStockRepo = binStockRepo;
        _txnRepo = txnRepo;
        _approvalService = approvalService;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<StockAdjustment> GetByIdAsync(int id)
    {
        var adj = await _adjRepo.GetQueryable()
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Product)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Batch)
            .Include(a => a.Lines)
                .ThenInclude(l => l.Bin)
            .FirstOrDefaultAsync(a => a.Id == id);

        return adj ?? throw new KeyNotFoundException($"Không tìm thấy biên bản điều chỉnh ID={id}.");
    }

    public async Task<StockAdjustment> CreateAsync(CreateAdjustmentDto dto, int userId)
    {
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");
        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException("Biên bản điều chỉnh phải có ít nhất 1 dòng.");

        var adjNumber = $"ADJ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var lines = new List<StockAdjustmentLine>();

        foreach (var l in dto.Lines)
        {
            if (!l.BatchId.HasValue)
                throw new InvalidOperationException("Mỗi dòng điều chỉnh phải chọn BatchId.");

            var currentStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == l.BinId &&
                bs.BatchId == l.BatchId.Value &&
                bs.ProductId == l.ProductId);

            decimal beforeQty = currentStock?.Quantity ?? 0;
            decimal afterQty = beforeQty + l.AdjustQty;

            lines.Add(new StockAdjustmentLine
            {
                BinId = l.BinId,
                ProductId = l.ProductId,
                BatchId = l.BatchId.Value,
                BeforeQty = beforeQty,
                AfterQty = afterQty,
                Notes = l.Reason
            });
        }

        var adj = new StockAdjustment
        {
            AdjNumber = adjNumber,
            WarehouseId = dto.WarehouseId,
            CountId = dto.StockCountId,
            Reason = dto.Reason ?? "Điều chỉnh kho",
            Notes = dto.Notes,
            Status = DocumentStatus.Draft,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Lines = lines
        };

        await _adjRepo.AddAsync(adj);
        await _adjRepo.SaveChangesAsync();
        return adj;
    }

    public async Task SubmitAsync(int id, int userId)
    {
        var adj = await GetByIdAsync(id);
        if (adj.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Chỉ biên bản DRAFT mới có thể Submit.");

        adj.Status = DocumentStatus.PendingL1;
        _adjRepo.Update(adj);
        await _adjRepo.SaveChangesAsync();

        await _approvalService.CreateApprovalRequestAsync(DocumentType.StockAdjustment, adj.Id, userId);
    }

    public async Task FinalizeAsync(int adjustmentId)
    {
        var adj = await GetByIdAsync(adjustmentId);
        if (adj.Status == DocumentStatus.Approved) return;

        adj.Status = DocumentStatus.Approved;
        adj.ApprovedAt = DateTime.UtcNow;

        foreach (var line in adj.Lines)
        {
            var binStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.BinId &&
                bs.BatchId == line.BatchId &&
                bs.ProductId == line.ProductId);

            decimal qtyBefore = binStock?.Quantity ?? 0;

            if (binStock == null)
            {
                binStock = new BinStock
                {
                    BinId = line.BinId,
                    BatchId = line.BatchId,
                    ProductId = line.ProductId,
                    Quantity = line.AfterQty,
                    ReservedQty = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _binStockRepo.AddAsync(binStock);
            }
            else
            {
                binStock.Quantity = line.AfterQty;
                binStock.UpdatedAt = DateTime.UtcNow;
                _binStockRepo.Update(binStock);
            }

            decimal delta = line.AfterQty - qtyBefore;
            var txnType = delta >= 0 ? StockTxnType.AdjIn : StockTxnType.AdjOut;

            var txn = new StockTransaction
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                BinId = line.BinId,
                TxnType = txnType,
                DocumentType = DocumentType.StockAdjustment,
                DocumentId = adj.Id,
                DocumentNumber = adj.AdjNumber,
                Quantity = Math.Abs(delta),
                QtyBefore = qtyBefore,
                QtyAfter = line.AfterQty,
                Remarks = $"Điều chỉnh tồn kho theo biên bản {adj.AdjNumber}",
                CreatedBy = adj.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _txnRepo.AddAsync(txn);
        }

        _adjRepo.Update(adj);
        await _adjRepo.SaveChangesAsync();
        await _binStockRepo.SaveChangesAsync();
    }
}
