using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class TransferOrderService : ITransferOrderService
{
    private readonly ITransferOrderRepository _transferRepo;
    private readonly IBinStockRepository _binStockRepo;
    private readonly IStockTransactionRepository _txnRepo;
    private readonly IApprovalService _approvalService;
    private readonly IWarehouseRepository _warehouseRepo;

    public TransferOrderService(
        ITransferOrderRepository transferRepo,
        IBinStockRepository binStockRepo,
        IStockTransactionRepository txnRepo,
        IApprovalService approvalService,
        IWarehouseRepository warehouseRepo)
    {
        _transferRepo = transferRepo;
        _binStockRepo = binStockRepo;
        _txnRepo = txnRepo;
        _approvalService = approvalService;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<TransferOrder> GetByIdAsync(int id)
    {
        var transfer = await _transferRepo.GetQueryable()
            .Include(t => t.Warehouse)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Lines)
                .ThenInclude(l => l.Product)
            .Include(t => t.Lines)
                .ThenInclude(l => l.Batch)
            .Include(t => t.Lines)
                .ThenInclude(l => l.FromBin)
            .Include(t => t.Lines)
                .ThenInclude(l => l.ToBin)
            .FirstOrDefaultAsync(t => t.Id == id);

        return transfer ?? throw new KeyNotFoundException($"Không tìm thấy phiếu chuyển kho ID={id}.");
    }

    public async Task<TransferOrder> CreateAsync(CreateTransferDto dto, int userId)
    {
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");
        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException("Phiếu chuyển kho phải có ít nhất 1 dòng.");

        // Kiểm tra tồn kho khả dụng tại FromBin
        foreach (var line in dto.Lines)
        {
            if (!line.BatchId.HasValue)
                throw new InvalidOperationException("Mỗi dòng chuyển kho phải chỉ định BatchId.");

            var fromStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.FromBinId &&
                bs.BatchId == line.BatchId.Value &&
                bs.ProductId == line.ProductId)
                ?? throw new InvalidOperationException($"Không tìm thấy hàng tại Bin nguồn (Bin={line.FromBinId}).");

            if (fromStock.Quantity - fromStock.ReservedQty < line.Quantity)
                throw new InvalidOperationException($"Bin nguồn {line.FromBinId} không đủ số lượng khả dụng.");
        }

        var transferNumber = $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var transfer = new TransferOrder
        {
            TransferNumber = transferNumber,
            WarehouseId = dto.WarehouseId,
            TransferDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Reason = dto.Notes,
            Status = DocumentStatus.Draft,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new TransferOrderLine
            {
                ProductId = l.ProductId,
                BatchId = l.BatchId!.Value,
                FromBinId = l.FromBinId,
                ToBinId = l.ToBinId,
                Quantity = l.Quantity
            }).ToList()
        };

        await _transferRepo.AddAsync(transfer);
        await _transferRepo.SaveChangesAsync();
        return transfer;
    }

    public async Task SubmitAsync(int id, int userId)
    {
        var transfer = await GetByIdAsync(id);
        if (transfer.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Chỉ phiếu DRAFT mới có thể Submit.");

        transfer.Status = DocumentStatus.PendingL1;
        transfer.UpdatedAt = DateTime.UtcNow;
        _transferRepo.Update(transfer);
        await _transferRepo.SaveChangesAsync();

        await _approvalService.CreateApprovalRequestAsync(DocumentType.Transfer, transfer.Id, userId);
    }

    public async Task CancelAsync(int id, int userId)
    {
        var transfer = await GetByIdAsync(id);
        if (transfer.Status == DocumentStatus.Approved || transfer.Status == DocumentStatus.Cancelled)
            throw new InvalidOperationException("Không thể hủy phiếu đã duyệt hoặc đã bị hủy.");

        transfer.Status = DocumentStatus.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;
        _transferRepo.Update(transfer);
        await _transferRepo.SaveChangesAsync();
    }

    /// <summary>
    /// Được gọi bởi ApprovalService sau khi được duyệt.
    /// Trừ tồn FromBin, cộng tồn ToBin, ghi 2 StockTransaction (TRANSFER_OUT & TRANSFER_IN).
    /// </summary>
    public async Task FinalizeAsync(int transferId)
    {
        var transfer = await GetByIdAsync(transferId);
        if (transfer.Status == DocumentStatus.Approved) return;

        transfer.Status = DocumentStatus.Approved;
        transfer.CompletedAt = DateTime.UtcNow;
        transfer.UpdatedAt = DateTime.UtcNow;

        foreach (var line in transfer.Lines)
        {
            // 1. Trừ FromBin
            var fromStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.FromBinId &&
                bs.BatchId == line.BatchId &&
                bs.ProductId == line.ProductId)
                ?? throw new InvalidOperationException($"Không tìm thấy tồn kho tại Bin nguồn {line.FromBinId}.");

            decimal fromBefore = fromStock.Quantity;
            fromStock.Quantity = Math.Max(0, fromStock.Quantity - line.Quantity);
            fromStock.UpdatedAt = DateTime.UtcNow;
            _binStockRepo.Update(fromStock);

            // Ghi Txn TransferOut
            var txnOut = new StockTransaction
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                BinId = line.FromBinId,
                TxnType = StockTxnType.TransferOut,
                DocumentType = DocumentType.Transfer,
                DocumentId = transfer.Id,
                DocumentNumber = transfer.TransferNumber,
                Quantity = line.Quantity,
                QtyBefore = fromBefore,
                QtyAfter = fromStock.Quantity,
                Remarks = $"Chuyển sang Bin {line.ToBinId}",
                CreatedBy = transfer.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _txnRepo.AddAsync(txnOut);

            // 2. Cộng ToBin
            var toStock = await _binStockRepo.FindAsync(bs =>
                bs.BinId == line.ToBinId &&
                bs.BatchId == line.BatchId &&
                bs.ProductId == line.ProductId);

            decimal toBefore = toStock?.Quantity ?? 0;

            if (toStock == null)
            {
                toStock = new BinStock
                {
                    BinId = line.ToBinId,
                    BatchId = line.BatchId,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    ReservedQty = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _binStockRepo.AddAsync(toStock);
            }
            else
            {
                toStock.Quantity += line.Quantity;
                toStock.UpdatedAt = DateTime.UtcNow;
                _binStockRepo.Update(toStock);
            }

            // Ghi Txn TransferIn
            var txnIn = new StockTransaction
            {
                ProductId = line.ProductId,
                BatchId = line.BatchId,
                BinId = line.ToBinId,
                TxnType = StockTxnType.TransferIn,
                DocumentType = DocumentType.Transfer,
                DocumentId = transfer.Id,
                DocumentNumber = transfer.TransferNumber,
                Quantity = line.Quantity,
                QtyBefore = toBefore,
                QtyAfter = toStock.Quantity,
                Remarks = $"Nhận chuyển từ Bin {line.FromBinId}",
                CreatedBy = transfer.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _txnRepo.AddAsync(txnIn);
        }

        _transferRepo.Update(transfer);
        await _transferRepo.SaveChangesAsync();
        await _binStockRepo.SaveChangesAsync();
    }
}
