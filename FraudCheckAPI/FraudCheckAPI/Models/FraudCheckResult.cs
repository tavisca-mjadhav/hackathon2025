namespace FraudCheckAPI.Models
{
    public class FraudCheckResult
    {
        public string OrderId { get; set; }
        public bool IsFraudulent { get; set; }
        public string Reason { get; set; }
    }
}
