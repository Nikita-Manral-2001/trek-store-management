namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class StoreCartItemsResponseViewModel
    {
        public List<StoreCartItemViewModel> Items { get; set; }
        public decimal GstTotalPrice { get; set; }
        public decimal GstPrice { get; set; }
      public decimal totalPrice { get; set; }
        public int ProductCount { get; set; }
    }
}
