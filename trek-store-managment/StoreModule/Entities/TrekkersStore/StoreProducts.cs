using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTH.Areas.Super.Data.Rent;
using TTH.Areas.Super.Models.Rent;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreProducts
    {
        [Key]
        public int Products_Id { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? CoverImgUrl { get; set; }

        [NotMapped]
        public IFormFile? CoverPhoto { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public StoreCategory StoreCategory { get; set; }
        public ICollection<ProductVariants> ProductVariants { get; set; }

        public bool IsVisible { get; set; }
    }
}
