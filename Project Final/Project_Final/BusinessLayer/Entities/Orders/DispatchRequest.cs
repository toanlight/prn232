using BusinessLayer.Common;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Orders;

public class DispatchRequest : BaseEntity
{
    public string RequestNumber { get; set; } = default!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateOnly RequestDate { get; set; }
    public DateOnly? RequiredDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "OPEN";   // OPEN|IN_PROGRESS|FULFILLED|CANCELLED
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public ICollection<GoodsDispatchNote> GDNs { get; set; } = new List<GoodsDispatchNote>();
}
