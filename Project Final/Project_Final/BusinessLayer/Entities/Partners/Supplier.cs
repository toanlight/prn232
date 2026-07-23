using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Stock;
using BusinessLayer.Entities.Orders;

namespace BusinessLayer.Entities.Partners;

public class Supplier : BaseEntity, IAuditable
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }

    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }

    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
