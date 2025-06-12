namespace PaymentApi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public Card Card { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class CardExpiry
    {
        public string Month { get; set; }
        public string Year { get; set; }
    }
    public class Card
    {
        public string Token { get; set; }
        public string Number { get; set; }
        public string IssuedBy { get; set; }
        public string HolderName { get; set; }
        public CardExpiry Expiry { get; set; }
    }
}
