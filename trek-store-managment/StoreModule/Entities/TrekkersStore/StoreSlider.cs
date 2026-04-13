namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreSlider
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Product_Id { get; set; }
        public string? DesktopImagePath { get; set; } // ⬅️ For Desktop
        public string? MobileImagePath { get; set; }  // ⬅️ For Mobile
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int SortOrder { get; set; }
    }
}
