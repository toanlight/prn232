using Microsoft.Extensions.DependencyInjection;
using Repositories.Implementations;
using Repositories.Interfaces;

namespace Repositories;

public static class RepositoryCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IZoneRepository, ZoneRepository>();
        services.AddScoped<IRackRepository, RackRepository>();
        services.AddScoped<IShelfRepository, ShelfRepository>();
        services.AddScoped<IBinRepository, BinRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();

        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IBinStockRepository, BinStockRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();

        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IGoodsReceiptNoteRepository, GoodsReceiptNoteRepository>();
        services.AddScoped<IGoodsDispatchNoteRepository, GoodsDispatchNoteRepository>();
        services.AddScoped<ITransferOrderRepository, TransferOrderRepository>();
        services.AddScoped<IDispatchRequestRepository, DispatchRequestRepository>();

        services.AddScoped<IApprovalRepository, ApprovalRepository>();

        services.AddScoped<IStockCountRepository, StockCountRepository>();
        services.AddScoped<IStockAdjustmentRepository, StockAdjustmentRepository>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

        return services;
    }
}
