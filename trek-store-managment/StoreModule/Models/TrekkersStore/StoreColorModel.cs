namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class StoreColorModel
    {
        public int IdColor { get; set; }
        public string Name { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsVisible { get; set; }
    }
}
