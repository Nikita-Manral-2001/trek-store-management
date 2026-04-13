namespace TTH.Areas.Super.Models.TrekkersStore
{
    public class PaymentSuccessDto
    {
        public string Razorpay_Payment_Id { get; set; }
        public string Razorpay_Order_Id { get; set; }
        public string Razorpay_Signature { get; set; }
        public string BookingId { get; set; }
    }
}
