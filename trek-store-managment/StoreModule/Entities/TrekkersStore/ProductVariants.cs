using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class ProductVariants
    {
        [Key]
        public int VariantId { get; set; }

        // Foreign Key for Product
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public StoreProducts StoreProduct { get; set; }

        // Foreign Key for Size
        public int SizeId { get; set; }
        [ForeignKey("SizeId")]
        public StoreSize StoreSize { get; set; }

        public string SizeName{get;set;}

        // Foreign Key for Color
        public int ColorId { get; set; }
        [ForeignKey("ColorId")]
        public StoreColor StoreColor { get; set; }
        public string ColorName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int Quantity { get; set; }
        public decimal  Price { get; set; }
        public bool IsVisible { get; set; } = true;
        
        public ICollection<StoreProductGallery> ProductGalleries { get; set; }
    }
}
