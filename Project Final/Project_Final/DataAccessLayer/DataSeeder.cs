using Microsoft.EntityFrameworkCore;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Products;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Entities.Approvals;
using BusinessLayer.Entities.System;
using BusinessLayer.Enums;

namespace DataAccessLayer;

public static class DataSeeder
{
    public static async Task SeedDataAsync(WmsDbContext context)
    {
        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { RoleCode = "ADMIN", RoleName = "Quản trị hệ thống", RoleNameEn = "System Admin", Description = "Toàn quyền trên hệ thống WMS", IsActive = true },
                new Role { RoleCode = "WH_MANAGER", RoleName = "Trưởng kho", RoleNameEn = "Warehouse Manager", Description = "Quản lý kho, phê duyệt L1 chứng từ", IsActive = true },
                new Role { RoleCode = "WH_STAFF", RoleName = "Nhân viên kho", RoleNameEn = "Warehouse Staff", Description = "Thực hiện nhập, xuất, picking, kiểm kê", IsActive = true },
                new Role { RoleCode = "DIRECTOR", RoleName = "Giám đốc", RoleNameEn = "Director", Description = "Phê duyệt L2 các chứng từ giá trị cao", IsActive = true },
                new Role { RoleCode = "PURCHASING", RoleName = "Nhân viên Mua hàng", RoleNameEn = "Purchasing Staff", Description = "Quản lý Đơn mua hàng PO & Nhà cung cấp", IsActive = true },
                new Role { RoleCode = "SALES", RoleName = "Nhân viên Kinh doanh", RoleNameEn = "Sales Staff", Description = "Tạo Yêu cầu xuất kho & Khách hàng", IsActive = true },
                new Role { RoleCode = "INV_CTRL", RoleName = "Kiểm soát Tồn kho", RoleNameEn = "Inventory Controller", Description = "Theo dõi tồn kho, hạn sử dụng & điều chỉnh", IsActive = true }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Users (Password default: "123456" hashed with BCrypt)
        string defaultPasswordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";

        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new User { Username = "admin", PasswordHash = defaultPasswordHash, FullName = "Quản trị viên Hệ thống", FullNameEn = "System Admin", Email = "admin@wms.com.vn", Phone = "0901234567", IsActive = true, PreferredLang = "vi", CreatedAt = DateTime.UtcNow },
                new User { Username = "manager01", PasswordHash = defaultPasswordHash, FullName = "Nguyễn Văn A (Trưởng Kho)", FullNameEn = "Nguyen Van A", Email = "manager01@wms.com.vn", Phone = "0902345678", IsActive = true, PreferredLang = "vi", CreatedAt = DateTime.UtcNow },
                new User { Username = "staff01", PasswordHash = defaultPasswordHash, FullName = "Trần Văn B (Thủ Kho)", FullNameEn = "Tran Van B", Email = "staff01@wms.com.vn", Phone = "0903456789", IsActive = true, PreferredLang = "vi", CreatedAt = DateTime.UtcNow },
                new User { Username = "director01", PasswordHash = defaultPasswordHash, FullName = "Lê Văn C (Giám Đốc)", FullNameEn = "Le Van C", Email = "director01@wms.com.vn", Phone = "0904567890", IsActive = true, PreferredLang = "vi", CreatedAt = DateTime.UtcNow },
                new User { Username = "purchasing01", PasswordHash = defaultPasswordHash, FullName = "Phạm Thị D (Mua Hàng)", FullNameEn = "Pham Thi D", Email = "purchasing01@wms.com.vn", Phone = "0905678901", IsActive = true, PreferredLang = "vi", CreatedAt = DateTime.UtcNow },
                new User { Username = "sales01", PasswordHash = defaultPasswordHash, FullName = "Hoàng Văn E (Kinh Doanh)", FullNameEn = "Hoang Van E", Email = "sales01@wms.com.vn", Phone = "0906789012", IsActive = true, PreferredLang = "vi", CreatedAt = DateTime.UtcNow }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Assign Roles
            var adminUser = await context.Users.FirstAsync(u => u.Username == "admin");
            var managerUser = await context.Users.FirstAsync(u => u.Username == "manager01");
            var staffUser = await context.Users.FirstAsync(u => u.Username == "staff01");
            var directorUser = await context.Users.FirstAsync(u => u.Username == "director01");
            var purchasingUser = await context.Users.FirstAsync(u => u.Username == "purchasing01");
            var salesUser = await context.Users.FirstAsync(u => u.Username == "sales01");

            var adminRole = await context.Roles.FirstAsync(r => r.RoleCode == "ADMIN");
            var managerRole = await context.Roles.FirstAsync(r => r.RoleCode == "WH_MANAGER");
            var staffRole = await context.Roles.FirstAsync(r => r.RoleCode == "WH_STAFF");
            var directorRole = await context.Roles.FirstAsync(r => r.RoleCode == "DIRECTOR");
            var purchasingRole = await context.Roles.FirstAsync(r => r.RoleCode == "PURCHASING");
            var salesRole = await context.Roles.FirstAsync(r => r.RoleCode == "SALES");

            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
                new UserRole { UserId = managerUser.Id, RoleId = managerRole.Id },
                new UserRole { UserId = staffUser.Id, RoleId = staffRole.Id },
                new UserRole { UserId = directorUser.Id, RoleId = directorRole.Id },
                new UserRole { UserId = purchasingUser.Id, RoleId = purchasingRole.Id },
                new UserRole { UserId = salesUser.Id, RoleId = salesRole.Id }
            };
            await context.UserRoles.AddRangeAsync(userRoles);
            await context.SaveChangesAsync();
        }

        // 3. Units of Measure (UoM)
        if (!await context.UnitsOfMeasure.AnyAsync())
        {
            var uoms = new List<UnitOfMeasure>
            {
                new UnitOfMeasure { Code = "PCS", Name = "Cái/Chiếc", NameEn = "Piece", IsActive = true },
                new UnitOfMeasure { Code = "BOX", Name = "Hộp", NameEn = "Box", IsActive = true },
                new UnitOfMeasure { Code = "CTN", Name = "Thùng", NameEn = "Carton", IsActive = true },
                new UnitOfMeasure { Code = "KG", Name = "Kilogram", NameEn = "Kilogram", IsActive = true },
                new UnitOfMeasure { Code = "BTL", Name = "Chai", NameEn = "Bottle", IsActive = true },
                new UnitOfMeasure { Code = "PAL", Name = "Pallet", NameEn = "Pallet", IsActive = true },
                new UnitOfMeasure { Code = "BAG", Name = "Túi", NameEn = "Bag", IsActive = true },
                new UnitOfMeasure { Code = "CBM", Name = "Mét khối", NameEn = "Cubic Meter", IsActive = true }
            };
            await context.UnitsOfMeasure.AddRangeAsync(uoms);
            await context.SaveChangesAsync();
        }

        // 4. Product Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<ProductCategory>
            {
                new ProductCategory { Code = "CAT-FOOD", Name = "Thực phẩm", NameEn = "Food & Grocery", IsActive = true, CreatedAt = DateTime.UtcNow },
                new ProductCategory { Code = "CAT-BEV", Name = "Đồ uống", NameEn = "Beverages", IsActive = true, CreatedAt = DateTime.UtcNow },
                new ProductCategory { Code = "CAT-MILK", Name = "Sữa & Chế phẩm", NameEn = "Dairy Products", IsActive = true, CreatedAt = DateTime.UtcNow },
                new ProductCategory { Code = "CAT-CHEM", Name = "Hóa mỹ phẩm", NameEn = "Personal Care & Chemicals", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 5. Suppliers
        if (!await context.Suppliers.AnyAsync())
        {
            var suppliers = new List<Supplier>
            {
                new Supplier { Code = "SUP-001", Name = "Công ty TNHH THVL", TaxCode = "0101234567", Address = "Lô B2, KCN Cát Lái, TP.HCM", Email = "contact@thvl.com.vn", Phone = "028.3812.3456", ContactPerson = "Nguyễn Văn Hùng", ContractNumber = "HD-SUP-2026-01", Status = SupplierStatus.Active, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Supplier { Code = "SUP-002", Name = "Tập đoàn Thực phẩm Á Châu", TaxCode = "0309876543", Address = "123 Nguyễn Văn Linh, Q.7, TP.HCM", Email = "info@achau.vn", Phone = "028.3999.8888", ContactPerson = "Trần Thị Mai", ContractNumber = "HD-SUP-2026-02", Status = SupplierStatus.Active, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }

        // 6. Customers
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new Customer { Code = "CUST-001", Name = "Chuỗi Siêu thị Mega Market", CustomerType = CustomerType.B2BService, TaxCode = "0312345678", Address = "456 Xa lộ Hà Nội, TP. Thủ Đức", Email = "purchasing@megamarket.vn", Phone = "028.3777.6666", ContactPerson = "Lê Hoàng Nam", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Customer { Code = "CUST-002", Name = "Đại lý Ký gửi An Bình", CustomerType = CustomerType.Consignee, TaxCode = "0319876543", Address = "789 Lê Văn Việt, Q.9, TP.HCM", Email = "anbinh.store@gmail.com", Phone = "0908.123.456", ContactPerson = "Phạm Tuyết Anh", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }

        // 7. Warehouse & Hierarchy (Warehouse -> Zone -> Rack -> Shelf -> Bin)
        if (!await context.Warehouses.AnyAsync())
        {
            var managerUser = await context.Users.FirstAsync(u => u.Username == "manager01");

            var warehouse = new Warehouse
            {
                Code = "WH-CENTRAL",
                Name = "Kho Tổng Trung Tâm",
                Address = "Lô C4, Đường số 2, KCN Tân Bình, TP. Hồ Chí Minh",
                ManagerUserId = managerUser.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Warehouses.AddAsync(warehouse);
            await context.SaveChangesAsync();

            var zoneA = new Zone { WarehouseId = warehouse.Id, Code = "ZONE-A", Name = "Zone A - Kho Khô", ZoneType = "STORAGE", IsActive = true };
            var zoneB = new Zone { WarehouseId = warehouse.Id, Code = "ZONE-B", Name = "Zone B - Kho Lạnh", ZoneType = "COLD_STORAGE", IsActive = true };
            await context.Zones.AddRangeAsync(zoneA, zoneB);
            await context.SaveChangesAsync();

            var rack1 = new Rack { ZoneId = zoneA.Id, Code = "RACK-01", Name = "Kệ R1", IsActive = true };
            var rack2 = new Rack { ZoneId = zoneB.Id, Code = "RACK-02", Name = "Kệ Lạnh R2", IsActive = true };
            await context.Racks.AddRangeAsync(rack1, rack2);
            await context.SaveChangesAsync();

            var shelf1 = new Shelf { RackId = rack1.Id, Code = "SHELF-01", Name = "Tầng S1", IsActive = true };
            var shelf2 = new Shelf { RackId = rack1.Id, Code = "SHELF-02", Name = "Tầng S2", IsActive = true };
            await context.Shelves.AddRangeAsync(shelf1, shelf2);
            await context.SaveChangesAsync();

            var bin1 = new Bin { ShelfId = shelf1.Id, Code = "WH1-ZA-R1-S1-B01", Name = "Bin B-01 (A-R1-S1)", MaxCapacity = 1000, CapacityUnit = "KG", IsActive = true };
            var bin2 = new Bin { ShelfId = shelf2.Id, Code = "WH1-ZA-R1-S2-B02", Name = "Bin B-02 (A-R1-S2)", MaxCapacity = 1000, CapacityUnit = "KG", IsActive = true };
            await context.Bins.AddRangeAsync(bin1, bin2);
            await context.SaveChangesAsync();
        }

        // 8. Products
        if (!await context.Products.AnyAsync())
        {
            var foodCat = await context.Categories.FirstAsync(c => c.Code == "CAT-FOOD");
            var bevCat = await context.Categories.FirstAsync(c => c.Code == "CAT-BEV");
            var boxUom = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "BOX");
            var btlUom = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "BTL");
            var adminUser = await context.Users.FirstAsync(u => u.Username == "admin");

            var products = new List<Product>
            {
                new Product
                {
                    SKU = "SKU-FOOD-001",
                    Name = "Sữa tươi tiệt trùng 1L",
                    NameEn = "Fresh Whole Milk 1L",
                    Barcode = "8934567890123",
                    CategoryId = foodCat.Id,
                    UomId = boxUom.Id,
                    MinStock = 50,
                    ReorderPoint = 100,
                    IsBatchTracked = true,
                    IsExpiryTracked = true,
                    ExpiryWarningDays = 30,
                    IsActive = true,
                    CreatedBy = adminUser.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    SKU = "SKU-BEV-009",
                    Name = "Nước ép cam nguyên chất 500ml",
                    NameEn = "Pure Orange Juice 500ml",
                    Barcode = "8934567890999",
                    CategoryId = bevCat.Id,
                    UomId = btlUom.Id,
                    MinStock = 30,
                    ReorderPoint = 60,
                    IsBatchTracked = true,
                    IsExpiryTracked = true,
                    ExpiryWarningDays = 30,
                    IsActive = true,
                    CreatedBy = adminUser.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // 9. Batches & BinStock
        if (!await context.Batches.AnyAsync())
        {
            var product1 = await context.Products.FirstAsync(p => p.SKU == "SKU-FOOD-001");
            var product2 = await context.Products.FirstAsync(p => p.SKU == "SKU-BEV-009");
            var supplier = await context.Suppliers.FirstAsync();
            var staffUser = await context.Users.FirstAsync(u => u.Username == "staff01");
            var bin1 = await context.Bins.FirstAsync(b => b.Code == "WH1-ZA-R1-S1-B01");
            var bin2 = await context.Bins.FirstAsync(b => b.Code == "WH1-ZA-R1-S2-B02");

            var batch1 = new Batch
            {
                ProductId = product1.Id,
                SupplierId = supplier.Id,
                LotNumber = "LOT-2026-0815",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-5)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(22)),
                InitialQty = 250,
                Status = BatchStatus.Active,
                CreatedBy = staffUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            var batch2 = new Batch
            {
                ProductId = product2.Id,
                SupplierId = supplier.Id,
                LotNumber = "LOT-2026-0910",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(48)),
                InitialQty = 150,
                Status = BatchStatus.Active,
                CreatedBy = staffUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.Batches.AddRangeAsync(batch1, batch2);
            await context.SaveChangesAsync();

            // BinStocks
            var binStock1 = new BinStock
            {
                BinId = bin1.Id,
                ProductId = product1.Id,
                BatchId = batch1.Id,
                Quantity = 250,
                ReservedQty = 50,
                UpdatedAt = DateTime.UtcNow
            };

            var binStock2 = new BinStock
            {
                BinId = bin2.Id,
                ProductId = product2.Id,
                BatchId = batch2.Id,
                Quantity = 150,
                ReservedQty = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await context.BinStocks.AddRangeAsync(binStock1, binStock2);
            await context.SaveChangesAsync();
        }

        // 10. SystemSettings
        if (!await context.SystemSettings.AnyAsync())
        {
            var settings = new List<SystemSetting>
            {
                new SystemSetting { SettingKey = "SYS_APP_NAME", SettingValue = "WMS Enterprise", Description = "Tên hệ thống", UpdatedAt = DateTime.UtcNow },
                new SystemSetting { SettingKey = "SYS_EXPIRY_WARNING_DAYS", SettingValue = "30", Description = "Số ngày cảnh báo hạn sử dụng mặc định", UpdatedAt = DateTime.UtcNow },
                new SystemSetting { SettingKey = "SYS_DEFAULT_LANG", SettingValue = "vi", Description = "Ngôn ngữ mặc định", UpdatedAt = DateTime.UtcNow },
                new SystemSetting { SettingKey = "SYS_SMTP_HOST", SettingValue = "smtp.gmail.com", Description = "Máy chủ SMTP gửi mail", UpdatedAt = DateTime.UtcNow },
                new SystemSetting { SettingKey = "SYS_SMTP_PORT", SettingValue = "587", Description = "Cổng SMTP", UpdatedAt = DateTime.UtcNow }
            };
            await context.SystemSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }

        // 11. ApprovalWorkflows
        if (!await context.ApprovalWorkflows.AnyAsync())
        {
            var workflows = new List<ApprovalWorkflow>
            {
                new ApprovalWorkflow { DocumentType = DocumentType.Grn, Level = 1, ApproverRoleCode = "WH_MANAGER", Description = "Phê duyệt Phiếu nhập kho L1 - Trưởng kho", IsActive = true },
                new ApprovalWorkflow { DocumentType = DocumentType.Grn, Level = 2, ApproverRoleCode = "DIRECTOR", Description = "Phê duyệt Phiếu nhập kho L2 - Giám đốc (Giá trị cao)", IsActive = true },
                new ApprovalWorkflow { DocumentType = DocumentType.Gdn, Level = 1, ApproverRoleCode = "WH_MANAGER", Description = "Phê duyệt Phiếu xuất kho L1 - Trưởng kho", IsActive = true },
                new ApprovalWorkflow { DocumentType = DocumentType.Gdn, Level = 2, ApproverRoleCode = "DIRECTOR", Description = "Phê duyệt Phiếu xuất kho L2 - Giám đốc (Giá trị cao)", IsActive = true }
            };
            await context.ApprovalWorkflows.AddRangeAsync(workflows);
            await context.SaveChangesAsync();
        }
    }
}
