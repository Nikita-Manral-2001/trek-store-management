using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreSize
    {
        [Key]
        public int IdSize { get; set; }
        public string Name { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public ICollection<ProductVariants>? ProductVariants { get; set; }
        public bool IsVisible { get; set; }
    }
}
