using BusinessLayer.Entities.Partners;
using BusinessLayer.Enums;
using Moq;
using Repositories.Interfaces;
using Service.Implementations;
using Service.Interfaces;
using Xunit;

namespace WMS.Tests;

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _supplierRepoMock;
    private readonly SupplierService _supplierService;

    public SupplierServiceTests()
    {
        _supplierRepoMock = new Mock<ISupplierRepository>();
        _supplierService = new SupplierService(_supplierRepoMock.Object);
    }

    [Fact]
    public async Task DeleteSupplierAsync_WhenSupplierDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        int supplierId = 999;
        _supplierRepoMock.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync((Supplier?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _supplierService.DeleteSupplierAsync(supplierId));

        Assert.Contains("Không tìm thấy nhà cung cấp", exception.Message);
        _supplierRepoMock.Verify(r => r.Remove(It.IsAny<Supplier>()), Times.Never);
        _supplierRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteSupplierAsync_WhenSupplierHasSuppliedGoods_ThrowsInvalidOperationException()
    {
        // Arrange
        int supplierId = 1;
        var existingSupplier = new Supplier
        {
            Id = supplierId,
            Code = "SUP-001",
            Name = "Công ty Cung ứng Đã Giao Hàng",
            IsActive = true,
            Status = SupplierStatus.Active
        };

        _supplierRepoMock.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(existingSupplier);

        // Mock HasSuppliedGoodsAsync to return TRUE (meaning supplier has POs/GRNs/Batches)
        _supplierRepoMock.Setup(r => r.HasSuppliedGoodsAsync(supplierId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _supplierService.DeleteSupplierAsync(supplierId));

        Assert.Contains("Không thể xóa nhà cung cấp này vì đã có hàng hóa", exception.Message);
        
        // Verify that Remove was NEVER called (hard delete blocked!)
        _supplierRepoMock.Verify(r => r.Remove(It.IsAny<Supplier>()), Times.Never);
        _supplierRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteSupplierAsync_WhenSupplierHasNotSuppliedGoods_DeletesSupplierSuccessfully()
    {
        // Arrange
        int supplierId = 2;
        var unusedSupplier = new Supplier
        {
            Id = supplierId,
            Code = "SUP-002",
            Name = "Nhà cung cấp chưa giao hàng bao giờ",
            IsActive = true,
            Status = SupplierStatus.Active
        };

        _supplierRepoMock.Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(unusedSupplier);

        // Mock HasSuppliedGoodsAsync to return FALSE (0 goods supplied)
        _supplierRepoMock.Setup(r => r.HasSuppliedGoodsAsync(supplierId))
            .ReturnsAsync(false);

        _supplierRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _supplierService.DeleteSupplierAsync(supplierId);

        // Assert
        // Verify that supplier was PERMANENTLY REMOVED (Hard Delete)
        _supplierRepoMock.Verify(r => r.Remove(It.Is<Supplier>(s => s.Id == supplierId)), Times.Once);
        _supplierRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateSupplierAsync_WithInvalidPhone_ThrowsArgumentException()
    {
        // Arrange
        var invalidDto = new CreateSupplierDto(
            Code: "SUP-100",
            Name: "Nhà Cung Cấp Test",
            TaxCode: null,
            Address: null,
            Email: "test@example.com",
            Phone: "123", // Invalid short phone number
            Website: null,
            ContactPerson: null,
            ContactPhone: null,
            ContractNumber: null,
            ContractStartDate: null,
            ContractEndDate: null,
            PaymentTerms: null,
            Notes: null
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _supplierService.CreateSupplierAsync(invalidDto));

        Assert.Contains("Số điện thoại không đúng định dạng", exception.Message);
    }
}
