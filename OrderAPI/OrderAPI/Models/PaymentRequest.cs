using System.ComponentModel.DataAnnotations;

namespace OrderAPI.Models
{
    public class PaymentRequest
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string OrderId { get; set; }
        public Card Card { get; set; }
    }

    public class CardExpiry
    {
        public string Month { get; set; }
        public string Year { get; set; }
    }
    public class Card
    {
        public string CVV { get; set; }

        public string Number { get; set; }
        public string IssuedBy { get; set; }
        public string HolderName { get; set; }
        public CardExpiry Expiry { get; set; }
    }
}
