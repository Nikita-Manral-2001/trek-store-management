using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreCartItem
    {
        [Key]
        public int CartItem_Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int VariantId { get; set; }
        
        public int Quantity { get; set; }
    
        public decimal TotalPrice { get; set; }
        public string? UserId { get; set; }
   
        public string? Email { get; set; }
    
       
        public DateTime OrderDate { get; set; }
        public string BookingId { get; set; }
    
    }
}
