using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreProductGallery
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key for ProductVariants
        public int VariantId { get; set; }
        [ForeignKey("VariantId")]
        public ProductVariants ProductVariant { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public int SortOrder { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
