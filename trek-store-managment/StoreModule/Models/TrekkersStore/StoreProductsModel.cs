using System.ComponentModel.DataAnnotations.Schema;
using TTH.Areas.Super.Data.Rent;
using TTH.Areas.Super.Models.Rent;

namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class StoreProductsModel
    {
        public int Products_Id { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? CoverImgUrl { get; set; }
        public decimal? Price { get; set; }
      
        [NotMapped]
        public IFormFile? CoverPhoto { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int Category_Id { get; set; }
        public bool IsVisible { get; set; }
    }
}
