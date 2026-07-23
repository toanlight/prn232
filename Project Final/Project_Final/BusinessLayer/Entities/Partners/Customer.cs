using BusinessLayer.Common;
using BusinessLayer.Enums;
using BusinessLayer.Entities.Orders;

namespace BusinessLayer.Entities.Partners;

public class Customer : BaseEntity, IAuditable
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public CustomerType CustomerType { get; set; } = CustomerType.Consignee;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }

    // Dùng cho loại B2BService
    public string? ContractNumber { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? ServiceTerms { get; set; }
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GoodsDispatchNote> DispatchNotes { get; set; } = new List<GoodsDispatchNote>();
}
