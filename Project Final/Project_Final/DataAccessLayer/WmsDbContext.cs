using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Entities.Approvals;
using BusinessLayer.Entities.System;

namespace DataAccessLayer;

public class WmsDbContext : DbContext
{
    public WmsDbContext(DbContextOptions<WmsDbContext> options) : base(options)
    {
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Products
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    // Partners
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();

    // Warehouse Structure
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Rack> Racks => Set<Rack>();
    public DbSet<Shelf> Shelves => Set<Shelf>();
    public DbSet<Bin> Bins => Set<Bin>();

    // Orders & Documents
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<POLine> POLines => Set<POLine>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GRNLine> GRNLines => Set<GRNLine>();
    public DbSet<DispatchRequest> DispatchRequests => Set<DispatchRequest>();
    public DbSet<GoodsDispatchNote> GoodsDispatchNotes => Set<GoodsDispatchNote>();
    public DbSet<GDNLine> GDNLines => Set<GDNLine>();
    public DbSet<TransferOrder> TransferOrders => Set<TransferOrder>();
    public DbSet<TransferOrderLine> TransferOrderLines => Set<TransferOrderLine>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountLine> StockCountLines => Set<StockCountLine>();

    // Stock & Inventory
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<BinStock> BinStocks => Set<BinStock>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    // Approvals
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<Approval> Approvals => Set<Approval>();

    // System & Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Composite Key for UserRole
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // 2. Unique Indexes
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(r => r.RoleCode).IsUnique();
        modelBuilder.Entity<ProductCategory>().HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<UnitOfMeasure>().HasIndex(u => u.Code).IsUnique();
        modelBuilder.Entity<Supplier>().HasIndex(s => s.Code).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<Warehouse>().HasIndex(w => w.Code).IsUnique();
        modelBuilder.Entity<Bin>().HasIndex(b => b.Code).IsUnique();
        modelBuilder.Entity<PurchaseOrder>().HasIndex(po => po.PONumber).IsUnique();
        modelBuilder.Entity<GoodsReceiptNote>().HasIndex(grn => grn.GRNNumber).IsUnique();
        modelBuilder.Entity<DispatchRequest>().HasIndex(dr => dr.RequestNumber).IsUnique();
        modelBuilder.Entity<GoodsDispatchNote>().HasIndex(gdn => gdn.GDNNumber).IsUnique();
        modelBuilder.Entity<TransferOrder>().HasIndex(to => to.TransferNumber).IsUnique();
        modelBuilder.Entity<StockAdjustment>().HasIndex(sa => sa.AdjNumber).IsUnique();
        modelBuilder.Entity<StockCount>().HasIndex(sc => sc.CountNumber).IsUnique();

        // 3. Disable Cascade Delete for Foreign Keys to avoid SQL Server cycles
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Specific cascade setups for composite UserRole
        modelBuilder.Entity<UserRole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserRole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
