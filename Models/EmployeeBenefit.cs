using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AdventureWorksCommons.Models
{
    public class EmployeeBenefit
    {
        [Key]
        public int BenefitID { get; set; }

        public int EmployeeID { get; set; }

        public int ProductID { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public DateTime? RedeemedDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; }

        public bool Redeemed { get; set; } = false;

        public virtual Employee Employee { get; set; }

        public virtual Product Product { get; set; }
    }
}
