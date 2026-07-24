using BusinessLayer.Entities.Products;
using Moq;
using Repositories.Interfaces;
using Service.Implementations;
using Service.Interfaces;
using Xunit;

namespace WMS.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<IUnitOfMeasureRepository> _uomRepoMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _uomRepoMock = new Mock<IUnitOfMeasureRepository>();

        _productService = new ProductService(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _uomRepoMock.Object
        );
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        int productId = 999;
        _productRepoMock.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _productService.DeleteProductAsync(productId));

        Assert.Contains("Không tìm thấy sản phẩm", exception.Message);
        _productRepoMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
        _productRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductHasAssociatedRecords_ThrowsInvalidOperationException()
    {
        // Arrange
        int productId = 1;
        var existingProduct = new Product
        {
            Id = productId,
            SKU = "PROD-001",
            Name = "Sản phẩm đã có lô hàng tồn kho",
            IsActive = true
        };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        // Mock HasAssociatedRecordsAsync to return TRUE
        _productRepoMock.Setup(r => r.HasAssociatedRecordsAsync(productId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _productService.DeleteProductAsync(productId));

        Assert.Contains("Không thể xóa sản phẩm này vì đã có dữ liệu tồn kho", exception.Message);

        _productRepoMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
        _productRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductHasNoAssociatedRecords_DeletesProductSuccessfully()
    {
        // Arrange
        int productId = 2;
        var unusedProduct = new Product
        {
            Id = productId,
            SKU = "PROD-002",
            Name = "Sản phẩm mới tạo chưa có dữ liệu",
            IsActive = true
        };

        _productRepoMock.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(unusedProduct);

        _productRepoMock.Setup(r => r.HasAssociatedRecordsAsync(productId))
            .ReturnsAsync(false);

        _productRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _productService.DeleteProductAsync(productId);

        // Assert
        _productRepoMock.Verify(r => r.Remove(It.Is<Product>(p => p.Id == productId)), Times.Once);
        _productRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSKU_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateProductDto(
            SKU: "PROD-EXISTING",
            Name: "Sản phẩm trùng SKU",
            NameEn: null,
            Barcode: null,
            CategoryId: 1,
            UomId: 1,
            MinStock: 10,
            ReorderPoint: 20,
            IsBatchTracked: true,
            IsExpiryTracked: false,
            ExpiryWarningDays: 30,
            Description: null
        );

        _productRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _productService.CreateProductAsync(dto));

        Assert.Contains("đã tồn tại", exception.Message);
    }

    [Fact]
    public async Task CreateProductAsync_WithNonExistentCategory_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new CreateProductDto(
            SKU: "PROD-100",
            Name: "Sản phẩm test danh mục sai",
            NameEn: null,
            Barcode: null,
            CategoryId: 999, // Non-existent category
            UomId: 1,
            MinStock: 10,
            ReorderPoint: 20,
            IsBatchTracked: true,
            IsExpiryTracked: false,
            ExpiryWarningDays: 30,
            Description: null
        );

        _productRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        _categoryRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _productService.CreateProductAsync(dto));

        Assert.Contains("Không tìm thấy danh mục", exception.Message);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidInput_CreatesProductSuccessfully()
    {
        // Arrange
        var dto = new CreateProductDto(
            SKU: "prod-888",
            Name: "  Sữa Chua Nếp Cẩm  ",
            NameEn: "Yogurt Rice",
            Barcode: "8931234567890",
            CategoryId: 1,
            UomId: 2,
            MinStock: 15,
            ReorderPoint: 30,
            IsBatchTracked: true,
            IsExpiryTracked: true,
            ExpiryWarningDays: 20,
            Description: "Sản phẩm thực phẩm bổ dưỡng"
        );

        _productRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        _categoryRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>()))
            .ReturnsAsync(true);

        _uomRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UnitOfMeasure, bool>>>()))
            .ReturnsAsync(true);

        _productRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.CreateProductAsync(dto, userId: 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PROD-888", result.SKU);
        Assert.Equal("Sữa Chua Nếp Cẩm", result.Name);
        Assert.Equal(3, result.CreatedBy);
        Assert.True(result.IsActive);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _productRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
