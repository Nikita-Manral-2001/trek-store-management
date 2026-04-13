using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreCategory
    {
        [Key]
        public int Category_Id { get; set; }
        public string? CategoryName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CoverImgUrl { get; set; }

        [NotMapped]
        public IFormFile? CoverPhoto { get; set; }

        //public ICollection<StoreProducts>? StoreProducts { get; set; }
        public bool IsVisible { get; set; }
    }
}
