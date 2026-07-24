using BusinessLayer.Entities.Warehouses;
using Repositories.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IZoneRepository _zoneRepo;
    private readonly IRackRepository _rackRepo;
    private readonly IShelfRepository _shelfRepo;
    private readonly IBinRepository _binRepo;
    private readonly IBinStockRepository _binStockRepo;

    public WarehouseService(
        IWarehouseRepository warehouseRepo, IZoneRepository zoneRepo,
        IRackRepository rackRepo, IShelfRepository shelfRepo,
        IBinRepository binRepo, IBinStockRepository binStockRepo)
    {
        _warehouseRepo = warehouseRepo;
        _zoneRepo = zoneRepo;
        _rackRepo = rackRepo;
        _shelfRepo = shelfRepo;
        _binRepo = binRepo;
        _binStockRepo = binStockRepo;
    }

    // ─── Warehouse ───────────────────────────────────────────────────────────
    public async Task<(List<Warehouse> Items, int TotalCount)> SearchWarehousesAsync(
        string? keyword, bool? isActive, int pageIndex = 1, int pageSize = 10)
        => await _warehouseRepo.SearchAsync(keyword, isActive, pageIndex, pageSize);

    public async Task<List<Warehouse>> GetActiveWarehousesAsync()
        => await _warehouseRepo.GetActiveWarehousesAsync();

    public async Task<Warehouse> GetByIdAsync(int id)
        => await _warehouseRepo.GetByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy kho ID={id}.");

    public async Task<Warehouse> GetWithHierarchyAsync(int id)
        => await _warehouseRepo.GetWithHierarchyByIdAsync(id)
           ?? throw new KeyNotFoundException($"Không tìm thấy kho ID={id}.");

    public async Task<Warehouse> CreateWarehouseAsync(CreateWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException("Mã kho là bắt buộc.");
        var trimmedCode = dto.Code.Trim();
        if (trimmedCode.Length < 2 || trimmedCode.Length > 50)
            throw new ArgumentException("Mã kho phải từ 2 đến 50 ký tự.");

        if (await _warehouseRepo.ExistsAsync(w => w.Code.ToLower() == trimmedCode.ToLower()))
            throw new InvalidOperationException($"Mã kho '{trimmedCode}' đã tồn tại.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tên kho là bắt buộc.");
        var trimmedName = dto.Name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 150)
            throw new ArgumentException("Tên kho phải từ 2 đến 150 ký tự.");

        var warehouse = new Warehouse
        {
            Code = trimmedCode.ToUpper(),
            Name = trimmedName,
            Address = dto.Address?.Trim(),
            ManagerUserId = dto.ManagerUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _warehouseRepo.AddAsync(warehouse);
        await _warehouseRepo.SaveChangesAsync();
        return warehouse;
    }

    public async Task<Warehouse> UpdateWarehouseAsync(int id, UpdateWarehouseDto dto)
    {
        var warehouse = await _warehouseRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy kho ID={id}.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Tên kho là bắt buộc.");
        var trimmedName = dto.Name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 150)
            throw new ArgumentException("Tên kho phải từ 2 đến 150 ký tự.");

        warehouse.Name = trimmedName;
        warehouse.Address = dto.Address?.Trim();
        warehouse.ManagerUserId = dto.ManagerUserId;
        warehouse.IsActive = dto.IsActive;

        _warehouseRepo.Update(warehouse);
        await _warehouseRepo.SaveChangesAsync();
        return warehouse;
    }

    public async Task DeleteWarehouseAsync(int id)
    {
        var warehouse = await _warehouseRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy kho ID={id}.");

        var hasAssociatedRecords = await _warehouseRepo.HasAssociatedRecordsAsync(id);
        if (hasAssociatedRecords)
        {
            throw new InvalidOperationException("Không thể xóa kho này vì đã có khu vực kho (Zones), đơn mua hàng, phiếu nhập/xuất hoặc chứng từ liên quan.");
        }

        _warehouseRepo.Remove(warehouse);
        await _warehouseRepo.SaveChangesAsync();
    }

    // ─── Zone ────────────────────────────────────────────────────────────────
    public async Task<List<Zone>> GetZonesByWarehouseAsync(int warehouseId)
        => await _zoneRepo.GetByWarehouseIdAsync(warehouseId);

    public async Task<Zone> CreateZoneAsync(CreateZoneDto dto)
    {
        if (!await _warehouseRepo.ExistsAsync(w => w.Id == dto.WarehouseId))
            throw new KeyNotFoundException($"Không tìm thấy kho ID={dto.WarehouseId}.");
        if (await _zoneRepo.ExistsAsync(z => z.WarehouseId == dto.WarehouseId && z.Code.ToLower() == dto.Code.ToLower()))
            throw new InvalidOperationException($"Mã zone '{dto.Code}' đã tồn tại trong kho này.");

        var zone = new Zone
        {
            WarehouseId = dto.WarehouseId,
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            ZoneType = dto.ZoneType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _zoneRepo.AddAsync(zone);
        await _zoneRepo.SaveChangesAsync();
        return zone;
    }

    public async Task<Zone> UpdateZoneAsync(int id, UpdateZoneDto dto)
    {
        var zone = await _zoneRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy Zone ID={id}.");
        zone.Name = dto.Name;
        zone.ZoneType = dto.ZoneType;
        zone.IsActive = dto.IsActive;
        _zoneRepo.Update(zone);
        await _zoneRepo.SaveChangesAsync();
        return zone;
    }

    public async Task DeleteZoneAsync(int id)
    {
        var zone = await _zoneRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy Zone ID={id}.");

        // Kiểm tra còn hàng tồn trong zone không
        var hasStock = await _binStockRepo.ExistsAsync(bs =>
            bs.Bin.Shelf.Rack.ZoneId == id && bs.Quantity > 0);
        if (hasStock)
            throw new InvalidOperationException("Không thể xóa zone khi còn hàng tồn kho bên trong.");

        _zoneRepo.Remove(zone);
        await _zoneRepo.SaveChangesAsync();
    }

    // ─── Rack ────────────────────────────────────────────────────────────────
    public async Task<List<Rack>> GetRacksByZoneAsync(int zoneId)
        => await _rackRepo.GetByZoneIdAsync(zoneId);

    public async Task<Rack> CreateRackAsync(CreateRackDto dto)
    {
        if (!await _zoneRepo.ExistsAsync(z => z.Id == dto.ZoneId))
            throw new KeyNotFoundException($"Không tìm thấy Zone ID={dto.ZoneId}.");

        var rack = new Rack
        {
            ZoneId = dto.ZoneId,
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _rackRepo.AddAsync(rack);
        await _rackRepo.SaveChangesAsync();
        return rack;
    }

    public async Task<Rack> UpdateRackAsync(int id, UpdateRackDto dto)
    {
        var rack = await _rackRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy Rack ID={id}.");
        rack.Name = dto.Name;
        rack.IsActive = dto.IsActive;
        _rackRepo.Update(rack);
        await _rackRepo.SaveChangesAsync();
        return rack;
    }

    // ─── Shelf ───────────────────────────────────────────────────────────────
    public async Task<List<Shelf>> GetShelvesByRackAsync(int rackId)
        => await _shelfRepo.GetByRackIdAsync(rackId);

    public async Task<Shelf> CreateShelfAsync(CreateShelfDto dto)
    {
        if (!await _rackRepo.ExistsAsync(r => r.Id == dto.RackId))
            throw new KeyNotFoundException($"Không tìm thấy Rack ID={dto.RackId}.");

        var shelf = new Shelf
        {
            RackId = dto.RackId,
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _shelfRepo.AddAsync(shelf);
        await _shelfRepo.SaveChangesAsync();
        return shelf;
    }

    public async Task<Shelf> UpdateShelfAsync(int id, UpdateShelfDto dto)
    {
        var shelf = await _shelfRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy Shelf ID={id}.");
        shelf.Name = dto.Name;
        shelf.IsActive = dto.IsActive;
        _shelfRepo.Update(shelf);
        await _shelfRepo.SaveChangesAsync();
        return shelf;
    }

    // ─── Bin ─────────────────────────────────────────────────────────────────
    public async Task<List<Bin>> GetBinsByShelfAsync(int shelfId)
        => await _binRepo.GetByShelfIdAsync(shelfId);

    public async Task<Bin> CreateBinAsync(CreateBinDto dto)
    {
        if (!await _shelfRepo.ExistsAsync(s => s.Id == dto.ShelfId))
            throw new KeyNotFoundException($"Không tìm thấy Shelf ID={dto.ShelfId}.");

        var bin = new Bin
        {
            ShelfId = dto.ShelfId,
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            MaxCapacity = dto.MaxCapacity,
            CapacityUnit = dto.CapacityUnit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _binRepo.AddAsync(bin);
        await _binRepo.SaveChangesAsync();
        return bin;
    }

    public async Task<Bin> UpdateBinAsync(int id, UpdateBinDto dto)
    {
        var bin = await _binRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy Bin ID={id}.");
        bin.Name = dto.Name;
        bin.MaxCapacity = dto.MaxCapacity;
        bin.CapacityUnit = dto.CapacityUnit;
        bin.IsActive = dto.IsActive;
        _binRepo.Update(bin);
        await _binRepo.SaveChangesAsync();
        return bin;
    }
}
