using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Entities.Partners;
using BusinessLayer.Entities.Warehouses;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Orders;

public class DispatchRequest : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RequestNumber { get; set; } = default!;

    public int CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = default!;

    public int WarehouseId { get; set; }
    [ForeignKey(nameof(WarehouseId))]
    public Warehouse Warehouse { get; set; } = default!;

    public DateOnly RequestDate { get; set; }
    public DateOnly? RequiredDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "OPEN";

    public int CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public ICollection<GoodsDispatchNote> GDNs { get; set; } = new List<GoodsDispatchNote>();
}
