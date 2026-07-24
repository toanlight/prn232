using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Entities.Orders;

namespace BusinessLayer.Entities.Partners;

public class Supplier : BaseEntity, IAuditable
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = default!;

    [StringLength(50)]
    public string? TaxCode { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Website { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(20)]
    public string? ContactPhone { get; set; }

    [StringLength(50)]
    public string? ContractNumber { get; set; }

    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

    [StringLength(100)]
    public string? PaymentTerms { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? CreatedBy { get; set; }
    [ForeignKey(nameof(CreatedBy))]
    public User? CreatedByUser { get; set; }

    public int? UpdatedBy { get; set; }
    [ForeignKey(nameof(UpdatedBy))]
    public User? UpdatedByUser { get; set; }

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
