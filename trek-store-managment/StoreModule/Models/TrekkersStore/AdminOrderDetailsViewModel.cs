using TTH.Areas.Super.Data.TrekkersStore;
using TTH.Models;

namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class AdminOrderItemViewModel
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public string ImagePath { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerDay { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Total => Quantity * PricePerDay;
    }
    public class AdminOrderDetailsViewModel
    {
        public List<AdminOrderItemViewModel> OrderItems { get; set; }
        public StoreUserAddress Address { get; set; }
        public RazorPayModel Orders { get; set; }
        public string BookingId { get; set; }
    }
}
