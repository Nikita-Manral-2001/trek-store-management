namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class StoreCartItemViewModel
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string bookingid { get; set; }
        public string ImageUrl { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
    }
}
