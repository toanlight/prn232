using BusinessLayer.Entities.Partners;
using BusinessLayer.Enums;
using Moq;
using Repositories.Interfaces;
using Service.Implementations;
using Service.Interfaces;
using Xunit;

namespace WMS.Tests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _customerRepoMock = new Mock<ICustomerRepository>();
        _customerService = new CustomerService(_customerRepoMock.Object);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        int customerId = 999;
        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _customerService.DeleteCustomerAsync(customerId));

        Assert.Contains("Không tìm thấy khách hàng", exception.Message);
        _customerRepoMock.Verify(r => r.Remove(It.IsAny<Customer>()), Times.Never);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerHasDispatchedGoods_ThrowsInvalidOperationException()
    {
        // Arrange
        int customerId = 1;
        var existingCustomer = new Customer
        {
            Id = customerId,
            Code = "CUST-001",
            Name = "Khách hàng siêu thị ABC",
            IsActive = true,
            CustomerType = CustomerType.Consignee
        };

        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        // Mock HasDispatchedGoodsAsync to return TRUE
        _customerRepoMock.Setup(r => r.HasDispatchedGoodsAsync(customerId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _customerService.DeleteCustomerAsync(customerId));

        Assert.Contains("Không thể xóa khách hàng này vì đã có đơn xuất hàng", exception.Message);

        _customerRepoMock.Verify(r => r.Remove(It.IsAny<Customer>()), Times.Never);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerHasNoDispatchedGoods_DeletesCustomerSuccessfully()
    {
        // Arrange
        int customerId = 2;
        var unusedCustomer = new Customer
        {
            Id = customerId,
            Code = "CUST-002",
            Name = "Khách hàng mới chưa có đơn xuất",
            IsActive = true,
            CustomerType = CustomerType.B2BService
        };

        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(unusedCustomer);

        _customerRepoMock.Setup(r => r.HasDispatchedGoodsAsync(customerId))
            .ReturnsAsync(false);

        _customerRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        _customerRepoMock.Verify(r => r.Remove(It.Is<Customer>(c => c.Id == customerId)), Times.Once);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidPhone_ThrowsArgumentException()
    {
        // Arrange
        var invalidDto = new CreateCustomerDto(
            Code: "CUST-100",
            Name: "Khách hàng Test",
            TaxCode: null,
            Address: null,
            Email: "test@example.com",
            Phone: "123", // Invalid short phone number
            ContactPerson: null,
            ContactPhone: null,
            ContractNumber: null,
            ContractStartDate: null,
            ContractEndDate: null,
            ServiceTerms: null,
            Notes: null,
            CustomerType: "Consignee"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _customerService.CreateCustomerAsync(invalidDto));

        Assert.Contains("Số điện thoại không đúng định dạng", exception.Message);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var invalidDto = new CreateCustomerDto(
            Code: "CUST-101",
            Name: "Khách hàng Test Email",
            TaxCode: null,
            Address: null,
            Email: "invalid-email-format",
            Phone: "0901234567",
            ContactPerson: null,
            ContactPhone: null,
            ContractNumber: null,
            ContractStartDate: null,
            ContractEndDate: null,
            ServiceTerms: null,
            Notes: null,
            CustomerType: "Consignee"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _customerService.CreateCustomerAsync(invalidDto));

        Assert.Contains("Email không đúng định dạng", exception.Message);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithDuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateCustomerDto(
            Code: "CUST-EXISTING",
            Name: "Khách hàng trùng mã",
            TaxCode: null,
            Address: null,
            Email: null,
            Phone: "0901234567",
            ContactPerson: null,
            ContactPhone: null,
            ContractNumber: null,
            ContractStartDate: null,
            ContractEndDate: null,
            ServiceTerms: null,
            Notes: null,
            CustomerType: "Consignee"
        );

        _customerRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _customerService.CreateCustomerAsync(dto));

        Assert.Contains("đã tồn tại", exception.Message);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithValidInput_CreatesCustomerSuccessfully()
    {
        // Arrange
        var dto = new CreateCustomerDto(
            Code: "cust-999",
            Name: "  Khách hàng hợp lệ  ",
            TaxCode: "0101234567",
            Address: "123 Đường ABC, Hà Nội",
            Email: "contact@khachhang.com",
            Phone: "0912345678",
            ContactPerson: "Nguyễn Văn A",
            ContactPhone: "0987654321",
            ContractNumber: "HD-2026/01",
            ContractStartDate: DateTime.Now.AddDays(-10),
            ContractEndDate: DateTime.Now.AddYears(1),
            ServiceTerms: "Thanh toán 30 ngày",
            Notes: "Khách hàng VIP",
            CustomerType: "B2BService"
        );

        _customerRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(false);

        _customerRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _customerService.CreateCustomerAsync(dto, userId: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CUST-999", result.Code);
        Assert.Equal("Khách hàng hợp lệ", result.Name);
        Assert.Equal(CustomerType.B2BService, result.CustomerType);
        Assert.Equal(5, result.CreatedBy);
        Assert.True(result.IsActive);
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
