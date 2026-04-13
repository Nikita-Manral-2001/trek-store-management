namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class ProductVariantsModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public List<IFormFile>? ProductGalleries { get; set; }

    }
}
