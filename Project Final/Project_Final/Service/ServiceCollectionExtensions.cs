using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Service.Implementations;
using Service.Interfaces;

namespace Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        // 1. Add Repositories
        services.AddRepositories();

        // 2. Add Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IGrnService, GrnService>();
        services.AddScoped<IGdnService, GdnService>();
        services.AddScoped<ITransferOrderService, TransferOrderService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IStockCountService, StockCountService>();
        services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISettingService, SettingService>();

        return services;
    }
}
