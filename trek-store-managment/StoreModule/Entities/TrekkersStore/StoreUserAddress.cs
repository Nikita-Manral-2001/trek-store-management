using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.TrekkersStore
{
    public class StoreUserAddress
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string StreetAddress { get; set; }
        public int PinCode { get; set; }
        public string? CompanyName { get; set; }
        public string? Notes { get; set; }
        public List<StoreParticipants> StoreParticipants { get; set; }
    }
}
