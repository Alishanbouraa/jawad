using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JawadContractingApp.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModifiedDate { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}