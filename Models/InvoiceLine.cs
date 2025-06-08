using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksCommons.Models;

[Table("InvoiceLine", Schema = "igs")]
public partial class InvoiceLine
{
    [Key]
    [Column("InvoiceLineID")]
    public int InvoiceLineId { get; set; }

    [Column("InvoiceID")]
    public int? InvoiceId { get; set; }

    [Column("ProductID")]
    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    [Column(TypeName = "money")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "money")]
    public decimal? LineTotal { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoiceLines")]
    public virtual Invoice? Invoice { get; set; }
}
