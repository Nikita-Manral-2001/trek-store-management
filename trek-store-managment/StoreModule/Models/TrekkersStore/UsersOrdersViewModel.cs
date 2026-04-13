namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class UsersOrdersViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string BookingId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public int? AddressId { get; set; }
        public string OrderDate { get; set; }

        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string ImagePath { get; set; }
    }
}
