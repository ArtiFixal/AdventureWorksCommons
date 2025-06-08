using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksCommons.Models;

[Table("Invoice", Schema = "igs")]
public partial class Invoice
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("SalesOrderID")]
    public int SalesOrderId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? InvoiceDate { get; set; }

    [Column(TypeName = "money")]
    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Invoice")]
    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}
