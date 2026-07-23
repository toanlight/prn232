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
        string defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

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
        else
        {
            // Ensure existing users have valid password hash
            var existingUsers = await context.Users.ToListAsync();
            foreach (var user in existingUsers)
            {
                if (!BCrypt.Net.BCrypt.Verify("123456", user.PasswordHash))
                {
                    user.PasswordHash = defaultPasswordHash;
                }
            }
            await context.SaveChangesAsync();
        }

        // Fetch users references for foreign key assignments
        var admin = await context.Users.FirstAsync(u => u.Username == "admin");
        var manager = await context.Users.FirstAsync(u => u.Username == "manager01");
        var staff = await context.Users.FirstAsync(u => u.Username == "staff01");
        var director = await context.Users.FirstAsync(u => u.Username == "director01");
        var purchasing = await context.Users.FirstAsync(u => u.Username == "purchasing01");
        var sales = await context.Users.FirstAsync(u => u.Username == "sales01");

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
            var warehouse = new Warehouse
            {
                Code = "WH-CENTRAL",
                Name = "Kho Tổng Trung Tâm",
                Address = "Lô C4, Đường số 2, KCN Tân Bình, TP. Hồ Chí Minh",
                ManagerUserId = manager.Id,
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

            var b1 = new Bin { ShelfId = shelf1.Id, Code = "WH1-ZA-R1-S1-B01", Name = "Bin B-01 (A-R1-S1)", MaxCapacity = 1000, CapacityUnit = "KG", IsActive = true };
            var b2 = new Bin { ShelfId = shelf2.Id, Code = "WH1-ZA-R1-S2-B02", Name = "Bin B-02 (A-R1-S2)", MaxCapacity = 1000, CapacityUnit = "KG", IsActive = true };
            var b3 = new Bin { ShelfId = shelf2.Id, Code = "WH1-ZA-R1-S2-B03", Name = "Bin B-03 (A-R1-S2)", MaxCapacity = 500, CapacityUnit = "KG", IsActive = true };
            await context.Bins.AddRangeAsync(b1, b2, b3);
            await context.SaveChangesAsync();
        }

        // 8. Products
        if (!await context.Products.AnyAsync())
        {
            var foodCat = await context.Categories.FirstAsync(c => c.Code == "CAT-FOOD");
            var bevCat = await context.Categories.FirstAsync(c => c.Code == "CAT-BEV");
            var boxUom = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "BOX");
            var btlUom = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "BTL");
            var ctnUom = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "CTN");

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
                    CreatedBy = admin.Id,
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
                    CreatedBy = admin.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    SKU = "SKU-FOOD-002",
                    Name = "Bột mì đa dụng Meizan 1kg",
                    NameEn = "Meizan All Purpose Flour 1kg",
                    Barcode = "8934567890333",
                    CategoryId = foodCat.Id,
                    UomId = ctnUom.Id,
                    MinStock = 20,
                    ReorderPoint = 40,
                    IsBatchTracked = true,
                    IsExpiryTracked = true,
                    ExpiryWarningDays = 60,
                    IsActive = true,
                    CreatedBy = admin.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // Fetch master references
        var product1 = await context.Products.FirstAsync(p => p.SKU == "SKU-FOOD-001");
        var product2 = await context.Products.FirstAsync(p => p.SKU == "SKU-BEV-009");
        var product3 = await context.Products.FirstAsync(p => p.SKU == "SKU-FOOD-002");

        var supplier1 = await context.Suppliers.FirstAsync(s => s.Code == "SUP-001");
        var supplier2 = await context.Suppliers.FirstAsync(s => s.Code == "SUP-002");

        var customer1 = await context.Customers.FirstAsync(c => c.Code == "CUST-001");

        var warehouseMain = await context.Warehouses.FirstAsync(w => w.Code == "WH-CENTRAL");
        var bin1 = await context.Bins.FirstAsync(b => b.Code == "WH1-ZA-R1-S1-B01");
        var bin2 = await context.Bins.FirstAsync(b => b.Code == "WH1-ZA-R1-S2-B02");
        var bin3 = await context.Bins.FirstAsync(b => b.Code == "WH1-ZA-R1-S2-B03");

        // 9. Batches & BinStocks
        if (!await context.Batches.AnyAsync())
        {
            var batch1 = new Batch
            {
                ProductId = product1.Id,
                SupplierId = supplier1.Id,
                LotNumber = "LOT-2026-0815",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-5)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(22)), // Expiring soon <= 30 days
                InitialQty = 500,
                Status = BatchStatus.Active,
                CreatedBy = staff.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            var batch2 = new Batch
            {
                ProductId = product2.Id,
                SupplierId = supplier2.Id,
                LotNumber = "LOT-2026-0910",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(48)), // Normal
                InitialQty = 300,
                Status = BatchStatus.Active,
                CreatedBy = staff.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var batch3 = new Batch
            {
                ProductId = product3.Id,
                SupplierId = supplier1.Id,
                LotNumber = "LOT-2025-1201",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), // Expired
                InitialQty = 100,
                Status = BatchStatus.Expired,
                CreatedBy = staff.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            };

            await context.Batches.AddRangeAsync(batch1, batch2, batch3);
            await context.SaveChangesAsync();

            // BinStocks
            var binStock1 = new BinStock
            {
                BinId = bin1.Id,
                ProductId = product1.Id,
                BatchId = batch1.Id,
                Quantity = 350,
                ReservedQty = 50,
                UpdatedAt = DateTime.UtcNow
            };

            var binStock2 = new BinStock
            {
                BinId = bin2.Id,
                ProductId = product2.Id,
                BatchId = batch2.Id,
                Quantity = 200,
                ReservedQty = 20,
                UpdatedAt = DateTime.UtcNow
            };

            var binStock3 = new BinStock
            {
                BinId = bin3.Id,
                ProductId = product3.Id,
                BatchId = batch3.Id,
                Quantity = 100,
                ReservedQty = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await context.BinStocks.AddRangeAsync(binStock1, binStock2, binStock3);
            await context.SaveChangesAsync();
        }

        var batch1Ref = await context.Batches.FirstAsync(b => b.LotNumber == "LOT-2026-0815");
        var batch2Ref = await context.Batches.FirstAsync(b => b.LotNumber == "LOT-2026-0910");

        // 10. Purchase Orders (PO) & POLines
        if (!await context.PurchaseOrders.AnyAsync())
        {
            var po1 = new PurchaseOrder
            {
                PONumber = "PO-202607-00001",
                SupplierId = supplier1.Id,
                WarehouseId = warehouseMain.Id,
                OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
                ExpectedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                Notes = "Đơn mua hàng thực phẩm định kỳ tháng 7",
                Status = "APPROVED",
                CreatedBy = purchasing.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            };

            await context.PurchaseOrders.AddAsync(po1);
            await context.SaveChangesAsync();

            var poLine1 = new POLine
            {
                POId = po1.Id,
                ProductId = product1.Id,
                OrderedQty = 500,
                ReceivedQty = 500,
                UnitPrice = 25000,
                Notes = "Giao đủ 500 hộp"
            };

            await context.POLines.AddAsync(poLine1);
            await context.SaveChangesAsync();
        }

        var po1Ref = await context.PurchaseOrders.FirstAsync(po => po.PONumber == "PO-202607-00001");

        // 11. Goods Receipt Notes (GRN) & GRNLines
        if (!await context.GoodsReceiptNotes.AnyAsync())
        {
            var grn1 = new GoodsReceiptNote
            {
                GRNNumber = "GRN-202607-00001",
                POId = po1Ref.Id,
                SupplierId = supplier1.Id,
                WarehouseId = warehouseMain.Id,
                ReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
                Notes = "Nhập kho đợt 1 theo PO-202607-00001",
                Status = DocumentStatus.Approved,
                CreatedBy = staff.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CompletedAt = DateTime.UtcNow.AddDays(-5)
            };

            var grn2 = new GoodsReceiptNote
            {
                GRNNumber = "GRN-202607-00002",
                SupplierId = supplier2.Id,
                WarehouseId = warehouseMain.Id,
                ReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Notes = "Nhập kho trực tiếp nước ép cam",
                Status = DocumentStatus.PendingL1,
                CreatedBy = staff.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.GoodsReceiptNotes.AddRangeAsync(grn1, grn2);
            await context.SaveChangesAsync();

            var grnLine1 = new GRNLine
            {
                GRNId = grn1.Id,
                ProductId = product1.Id,
                BatchId = batch1Ref.Id,
                BinId = bin1.Id,
                Quantity = 500,
                LotNumber = "LOT-2026-0815",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-5)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(22)),
                UnitPrice = 25000,
                Notes = "Đã vào bin B01"
            };

            var grnLine2 = new GRNLine
            {
                GRNId = grn2.Id,
                ProductId = product2.Id,
                BatchId = batch2Ref.Id,
                BinId = bin2.Id,
                Quantity = 300,
                LotNumber = "LOT-2026-0910",
                MfgDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(48)),
                UnitPrice = 18000,
                Notes = "Chờ duyệt L1"
            };

            await context.GRNLines.AddRangeAsync(grnLine1, grnLine2);
            await context.SaveChangesAsync();
        }

        var grn1Ref = await context.GoodsReceiptNotes.FirstAsync(g => g.GRNNumber == "GRN-202607-00001");

        // 12. Dispatch Requests & Goods Dispatch Notes (GDN) & GDNLines
        if (!await context.DispatchRequests.AnyAsync())
        {
            var dr1 = new DispatchRequest
            {
                RequestNumber = "DR-202607-00001",
                CustomerId = customer1.Id,
                WarehouseId = warehouseMain.Id,
                RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                RequiredDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                Notes = "Xuất kho siêu thị Mega Market",
                Status = "APPROVED",
                CreatedBy = sales.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            await context.DispatchRequests.AddAsync(dr1);
            await context.SaveChangesAsync();
        }

        var dr1Ref = await context.DispatchRequests.FirstAsync(dr => dr.RequestNumber == "DR-202607-00001");

        if (!await context.GoodsDispatchNotes.AnyAsync())
        {
            var gdn1 = new GoodsDispatchNote
            {
                GDNNumber = "GDN-202607-00001",
                RequestId = dr1Ref.Id,
                CustomerId = customer1.Id,
                WarehouseId = warehouseMain.Id,
                DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                DeliveryAddress = "456 Xa lộ Hà Nội, TP. Thủ Đức",
                Notes = "Đã hoàn thành xuất kho & giao đơn",
                Status = DocumentStatus.Delivered,
                PickedBy = staff.Id,
                PickedAt = DateTime.UtcNow.AddDays(-2),
                DeliveredBy = staff.Id,
                DeliveredAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = sales.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var gdn2 = new GoodsDispatchNote
            {
                GDNNumber = "GDN-202607-00002",
                CustomerId = customer1.Id,
                WarehouseId = warehouseMain.Id,
                DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DeliveryAddress = "789 Lê Văn Việt, Q.9, TP.HCM",
                Notes = "Đơn xuất kho gấp chờ duyệt L2 Giám đốc",
                Status = DocumentStatus.PendingL2,
                CreatedBy = sales.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.GoodsDispatchNotes.AddRangeAsync(gdn1, gdn2);
            await context.SaveChangesAsync();

            var gdnLine1 = new GDNLine
            {
                GDNId = gdn1.Id,
                ProductId = product1.Id,
                BatchId = batch1Ref.Id,
                BinId = bin1.Id,
                RequestedQty = 150,
                PickedQty = 150,
                Notes = "FEFO xuất lô hết hạn trước"
            };

            var gdnLine2 = new GDNLine
            {
                GDNId = gdn2.Id,
                ProductId = product2.Id,
                BatchId = batch2Ref.Id,
                BinId = bin2.Id,
                RequestedQty = 100,
                PickedQty = 0,
                Notes = "Chờ duyệt L2"
            };

            await context.GDNLines.AddRangeAsync(gdnLine1, gdnLine2);
            await context.SaveChangesAsync();
        }

        var gdn1Ref = await context.GoodsDispatchNotes.FirstAsync(g => g.GDNNumber == "GDN-202607-00001");

        // 13. Transfer Orders & Lines
        if (!await context.TransferOrders.AnyAsync())
        {
            var transfer1 = new TransferOrder
            {
                TransferNumber = "TR-202607-00001",
                WarehouseId = warehouseMain.Id,
                TransferDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                Reason = "Điều chuyển nội bộ từ Bin B01 sang Bin B02 để đảo kho",
                Status = DocumentStatus.Completed,
                CreatedBy = staff.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            };

            await context.TransferOrders.AddAsync(transfer1);
            await context.SaveChangesAsync();

            var transferLine1 = new TransferOrderLine
            {
                TransferId = transfer1.Id,
                ProductId = product1.Id,
                BatchId = batch1Ref.Id,
                FromBinId = bin1.Id,
                ToBinId = bin2.Id,
                Quantity = 50,
                Notes = "Đã hoàn thành đảo vị trí"
            };

            await context.TransferOrderLines.AddAsync(transferLine1);
            await context.SaveChangesAsync();
        }

        // 14. Stock Counts & Lines
        if (!await context.StockCounts.AnyAsync())
        {
            var count1 = new StockCount
            {
                CountNumber = "SC-202607-00001",
                WarehouseId = warehouseMain.Id,
                CountType = CountType.Periodic,
                CountDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-4)),
                PlannedBy = manager.Id,
                Notes = "Kiểm kê định kỳ tháng 7 Zone A",
                Status = "COMPLETED",
                StartedAt = DateTime.UtcNow.AddDays(-4),
                CompletedAt = DateTime.UtcNow.AddDays(-4),
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            await context.StockCounts.AddAsync(count1);
            await context.SaveChangesAsync();

            var countLine1 = new StockCountLine
            {
                CountId = count1.Id,
                BinId = bin1.Id,
                ProductId = product1.Id,
                BatchId = batch1Ref.Id,
                SystemQty = 350,
                ActualQty = 348, // Variance -2
                CountedBy = staff.Id,
                CountedAt = DateTime.UtcNow.AddDays(-4),
                Notes = "Hao hụt 2 hộp do rách bao bì"
            };

            await context.StockCountLines.AddAsync(countLine1);
            await context.SaveChangesAsync();
        }

        var count1Ref = await context.StockCounts.FirstAsync(sc => sc.CountNumber == "SC-202607-00001");

        // 15. Stock Adjustments & Lines
        if (!await context.StockAdjustments.AnyAsync())
        {
            var adj1 = new StockAdjustment
            {
                AdjNumber = "SA-202607-00001",
                CountId = count1Ref.Id,
                WarehouseId = warehouseMain.Id,
                Reason = "Điều chỉnh giảm 2 hộp do hư hỏng kiểm kê SC-202607-00001",
                Notes = "Đã duyệt điều chỉnh kho",
                Status = DocumentStatus.Approved,
                CreatedBy = manager.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                ApprovedAt = DateTime.UtcNow.AddDays(-4)
            };

            await context.StockAdjustments.AddAsync(adj1);
            await context.SaveChangesAsync();

            var adjLine1 = new StockAdjustmentLine
            {
                AdjustmentId = adj1.Id,
                BinId = bin1.Id,
                ProductId = product1.Id,
                BatchId = batch1Ref.Id,
                BeforeQty = 350,
                AfterQty = 348,
                Notes = "Xuất hủy do hư hỏng"
            };

            await context.StockAdjustmentLines.AddAsync(adjLine1);
            await context.SaveChangesAsync();
        }

        // 16. Stock Transactions (Lịch sử giao dịch nhập/xuất để vẽ biểu đồ Dashboard)
        if (!await context.StockTransactions.AnyAsync())
        {
            var txns = new List<StockTransaction>
            {
                // Nhập kho 500 hộp sữa
                new StockTransaction
                {
                    ProductId = product1.Id,
                    BatchId = batch1Ref.Id,
                    BinId = bin1.Id,
                    TxnType = StockTxnType.GrnIn,
                    DocumentType = DocumentType.Grn,
                    DocumentId = grn1Ref.Id,
                    DocumentNumber = grn1Ref.GRNNumber,
                    Quantity = 500,
                    QtyBefore = 0,
                    QtyAfter = 500,
                    Remarks = "Nhập kho theo GRN-202607-00001",
                    CreatedBy = staff.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                // Xuất kho 150 hộp sữa
                new StockTransaction
                {
                    ProductId = product1.Id,
                    BatchId = batch1Ref.Id,
                    BinId = bin1.Id,
                    TxnType = StockTxnType.GdnOut,
                    DocumentType = DocumentType.Gdn,
                    DocumentId = gdn1Ref.Id,
                    DocumentNumber = gdn1Ref.GDNNumber,
                    Quantity = -150,
                    QtyBefore = 500,
                    QtyAfter = 350,
                    Remarks = "Xuất kho bán hàng theo GDN-202607-00001",
                    CreatedBy = staff.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await context.StockTransactions.AddRangeAsync(txns);
            await context.SaveChangesAsync();
        }

        // 17. SystemSettings
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

        // 18. ApprovalWorkflows & Approvals (Hộp thư duyệt L1/L2 thực tế)
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

        if (!await context.Approvals.AnyAsync())
        {
            var grn2Ref = await context.GoodsReceiptNotes.FirstAsync(g => g.GRNNumber == "GRN-202607-00002");
            var gdn2Ref = await context.GoodsDispatchNotes.FirstAsync(g => g.GDNNumber == "GDN-202607-00002");

            var approvals = new List<Approval>
            {
                // Approval pending L1 for GRN-202607-00002
                new Approval
                {
                    DocumentType = DocumentType.Grn,
                    DocumentId = grn2Ref.Id,
                    Level = ApprovalLevel.L1Manager,
                    Status = ApprovalStatus.Pending,
                    Comment = "Yêu cầu Trưởng kho Nguyễn Văn A phê duyệt nhập kho đợt 2",
                    CreatedAt = DateTime.UtcNow
                },
                // Approval pending L2 for GDN-202607-00002
                new Approval
                {
                    DocumentType = DocumentType.Gdn,
                    DocumentId = gdn2Ref.Id,
                    Level = ApprovalLevel.L2Director,
                    Status = ApprovalStatus.Pending,
                    Comment = "Yêu cầu Giám đốc Lê Văn C phê duyệt xuất kho giá trị cao",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Approvals.AddRangeAsync(approvals);
            await context.SaveChangesAsync();
        }

        // 19. Notifications
        if (!await context.Notifications.AnyAsync())
        {
            var notifications = new List<Notification>
            {
                new Notification
                {
                    UserId = manager.Id,
                    NotifType = "APPROVAL_REQ",
                    Title = "Yêu cầu phê duyệt Phiếu Nhập GRN-202607-00002",
                    TitleEn = "Approval Request for GRN-202607-00002",
                    Body = "Phiếu nhập kho GRN-202607-00002 đang chờ bạn phê duyệt L1.",
                    BodyEn = "GRN-202607-00002 is waiting for your L1 approval.",
                    ReferenceType = "GRN",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    UserId = director.Id,
                    NotifType = "APPROVAL_REQ",
                    Title = "Yêu cầu phê duyệt Phiếu Xuất GDN-202607-00002",
                    TitleEn = "Approval Request for GDN-202607-00002",
                    Body = "Phiếu xuất kho GDN-202607-00002 giá trị lớn đang chờ bạn phê duyệt L2.",
                    BodyEn = "GDN-202607-00002 is waiting for your L2 approval.",
                    ReferenceType = "GDN",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    UserId = staff.Id,
                    NotifType = "EXPIRY_WARN",
                    Title = "Cảnh báo Lô hàng LOT-2026-0815 sắp hết hạn!",
                    TitleEn = "Expiry Warning for Lot LOT-2026-0815!",
                    Body = "Sản phẩm Sữa tươi 1L thuộc lô LOT-2026-0815 sẽ hết hạn trong vòng 22 ngày tới.",
                    BodyEn = "Product Fresh Whole Milk 1L lot LOT-2026-0815 will expire in 22 days.",
                    ReferenceType = "BATCH",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
        }
    }
}
