using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreParticipants
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public int VariantId { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
        public string? UserId { get; set; }

        public string? Email { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string BookingId { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public int? AddressId { get; set; }
        [ForeignKey("AddressId")]
        public StoreUserAddress StoreUserAddress { get; set; }
    }
}
