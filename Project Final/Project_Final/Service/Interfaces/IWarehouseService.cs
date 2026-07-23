using BusinessLayer.Entities.Warehouses;

namespace Service.Interfaces;

public interface IWarehouseService
{
    // Warehouse
    Task<List<Warehouse>> GetActiveWarehousesAsync();
    Task<Warehouse> GetByIdAsync(int id);
    Task<Warehouse> GetWithHierarchyAsync(int id);
    Task<Warehouse> CreateWarehouseAsync(CreateWarehouseDto dto);
    Task<Warehouse> UpdateWarehouseAsync(int id, UpdateWarehouseDto dto);

    // Zone
    Task<List<Zone>> GetZonesByWarehouseAsync(int warehouseId);
    Task<Zone> CreateZoneAsync(CreateZoneDto dto);
    Task<Zone> UpdateZoneAsync(int id, UpdateZoneDto dto);
    Task DeleteZoneAsync(int id);

    // Rack
    Task<List<Rack>> GetRacksByZoneAsync(int zoneId);
    Task<Rack> CreateRackAsync(CreateRackDto dto);
    Task<Rack> UpdateRackAsync(int id, UpdateRackDto dto);

    // Shelf
    Task<List<Shelf>> GetShelvesByRackAsync(int rackId);
    Task<Shelf> CreateShelfAsync(CreateShelfDto dto);
    Task<Shelf> UpdateShelfAsync(int id, UpdateShelfDto dto);

    // Bin
    Task<List<Bin>> GetBinsByShelfAsync(int shelfId);
    Task<Bin> CreateBinAsync(CreateBinDto dto);
    Task<Bin> UpdateBinAsync(int id, UpdateBinDto dto);
}

// DTOs
public record CreateWarehouseDto(string Code, string Name, string? Address, int? ManagerUserId);
public record UpdateWarehouseDto(string Name, string? Address, int? ManagerUserId, bool IsActive);
public record CreateZoneDto(int WarehouseId, string Code, string Name, string ZoneType);
public record UpdateZoneDto(string Name, string ZoneType, bool IsActive);
public record CreateRackDto(int ZoneId, string Code, string Name);
public record UpdateRackDto(string Name, bool IsActive);
public record CreateShelfDto(int RackId, string Code, string Name);
public record UpdateShelfDto(string Name, bool IsActive);
public record CreateBinDto(int ShelfId, string Code, string Name, decimal? MaxCapacity, string? CapacityUnit);
public record UpdateBinDto(string Name, decimal? MaxCapacity, string? CapacityUnit, bool IsActive);
