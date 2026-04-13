using Microsoft.AspNetCore.Mvc.Rendering;

namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class ProductVariantInputModel
    {
        public int ProductId { get; set; }

        public int Sizes { get; set; }
        public int Colors { get; set; }
        public int SizeId { get; set; }
        public string SizeName { get; set; }

        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public List<ProductVariantViewModel> ExistingVariants { get; set; } = new List<ProductVariantViewModel>();
    }
}
