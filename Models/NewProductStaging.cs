using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksCommons.Models;

[Keyless]
[Table("NewProductStaging", Schema = "prs")]
public partial class NewProductStaging
{
    [StringLength(60)]
    public string Name { get; set; } = null!;

    [StringLength(25)]
    public string ProductNumber { get; set; } = null!;

    public bool MakeFlag { get; set; }

    public bool FinishedGoodsFlag { get; set; }

    [StringLength(15)]
    public string? Color { get; set; }

    public short SafetyStockLevel { get; set; }

    public short ReorderPoint { get; set; }

    [Column(TypeName = "money")]
    public decimal StandardCost { get; set; }

    [Column(TypeName = "money")]
    public decimal ListPrice { get; set; }

    [StringLength(5)]
    public string? Size { get; set; }

    [StringLength(3)]
    public string? SizeUnitMeasureCode { get; set; }

    [StringLength(3)]
    public string? WeightUnitMeasureCode { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? Weight { get; set; }

    public int DaysToManufacture { get; set; }

    [StringLength(2)]
    public string? ProductLine { get; set; }

    [StringLength(2)]
    public string? Class { get; set; }

    [StringLength(2)]
    public string? Style { get; set; }

    [Column("ProductSubcategoryID")]
    public int? ProductSubcategoryId { get; set; }

    [Column("ProductModelID")]
    public int? ProductModelId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SellStartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SellEndDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DiscontinuedDate { get; set; }
}
