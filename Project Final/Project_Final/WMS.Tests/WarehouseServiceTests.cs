using BusinessLayer.Entities.Warehouses;
using Moq;
using Repositories.Interfaces;
using Service.Implementations;
using Service.Interfaces;
using Xunit;

namespace WMS.Tests;

public class WarehouseServiceTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
    private readonly Mock<IZoneRepository> _zoneRepoMock;
    private readonly Mock<IRackRepository> _rackRepoMock;
    private readonly Mock<IShelfRepository> _shelfRepoMock;
    private readonly Mock<IBinRepository> _binRepoMock;
    private readonly Mock<IBinStockRepository> _binStockRepoMock;
    private readonly WarehouseService _warehouseService;

    public WarehouseServiceTests()
    {
        _warehouseRepoMock = new Mock<IWarehouseRepository>();
        _zoneRepoMock = new Mock<IZoneRepository>();
        _rackRepoMock = new Mock<IRackRepository>();
        _shelfRepoMock = new Mock<IShelfRepository>();
        _binRepoMock = new Mock<IBinRepository>();
        _binStockRepoMock = new Mock<IBinStockRepository>();

        _warehouseService = new WarehouseService(
            _warehouseRepoMock.Object,
            _zoneRepoMock.Object,
            _rackRepoMock.Object,
            _shelfRepoMock.Object,
            _binRepoMock.Object,
            _binStockRepoMock.Object
        );
    }

    [Fact]
    public async Task DeleteWarehouseAsync_WhenWarehouseDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        int warehouseId = 999;
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId))
            .ReturnsAsync((Warehouse?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _warehouseService.DeleteWarehouseAsync(warehouseId));

        Assert.Contains("Không tìm thấy kho", exception.Message);
        _warehouseRepoMock.Verify(r => r.Remove(It.IsAny<Warehouse>()), Times.Never);
        _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteWarehouseAsync_WhenWarehouseHasAssociatedRecords_ThrowsInvalidOperationException()
    {
        // Arrange
        int warehouseId = 1;
        var existingWarehouse = new Warehouse
        {
            Id = warehouseId,
            Code = "WH-MAIN",
            Name = "Kho Tổng Hà Nội",
            IsActive = true
        };

        _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId))
            .ReturnsAsync(existingWarehouse);

        _warehouseRepoMock.Setup(r => r.HasAssociatedRecordsAsync(warehouseId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _warehouseService.DeleteWarehouseAsync(warehouseId));

        Assert.Contains("Không thể xóa kho này vì đã có khu vực kho", exception.Message);
        _warehouseRepoMock.Verify(r => r.Remove(It.IsAny<Warehouse>()), Times.Never);
        _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteWarehouseAsync_WhenWarehouseHasNoAssociatedRecords_DeletesWarehouseSuccessfully()
    {
        // Arrange
        int warehouseId = 2;
        var unusedWarehouse = new Warehouse
        {
            Id = warehouseId,
            Code = "WH-TEST",
            Name = "Kho thử nghiệm mới tạo",
            IsActive = true
        };

        _warehouseRepoMock.Setup(r => r.GetByIdAsync(warehouseId))
            .ReturnsAsync(unusedWarehouse);

        _warehouseRepoMock.Setup(r => r.HasAssociatedRecordsAsync(warehouseId))
            .ReturnsAsync(false);

        _warehouseRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _warehouseService.DeleteWarehouseAsync(warehouseId);

        // Assert
        _warehouseRepoMock.Verify(r => r.Remove(It.Is<Warehouse>(w => w.Id == warehouseId)), Times.Once);
        _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateWarehouseAsync_WithDuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateWarehouseDto(
            Code: "WH-MAIN",
            Name: "Kho Tổng Trùng Mã",
            Address: "Hà Nội",
            ManagerUserId: null
        );

        _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Warehouse, bool>>>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _warehouseService.CreateWarehouseAsync(dto));

        Assert.Contains("đã tồn tại", exception.Message);
    }

    [Fact]
    public async Task CreateWarehouseAsync_WithValidData_CreatesWarehouseSuccessfully()
    {
        // Arrange
        var dto = new CreateWarehouseDto(
            Code: "wh-south-01",
            Name: "  Kho Miền Nam 01  ",
            Address: "TP. Hồ Chí Minh",
            ManagerUserId: null
        );

        _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Warehouse, bool>>>()))
            .ReturnsAsync(false);

        _warehouseRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _warehouseService.CreateWarehouseAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WH-SOUTH-01", result.Code);
        Assert.Equal("Kho Miền Nam 01", result.Name);
        Assert.Equal("TP. Hồ Chí Minh", result.Address);
        Assert.True(result.IsActive);
        _warehouseRepoMock.Verify(r => r.AddAsync(It.IsAny<Warehouse>()), Times.Once);
        _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
