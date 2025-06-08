using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksCommons.Models;

[Table("StockChangeLog", Schema = "mms")]
public partial class StockChangeLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("LocationID")]
    public short LocationId { get; set; }

    public short? OldQuantity { get; set; }

    public short? NewQuantity { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ChangeDate { get; set; }

    [StringLength(128)]
    public string ChangedBy { get; set; } = null!;
}
