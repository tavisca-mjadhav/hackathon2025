using OrderAPI.Models;
public class FraudCheckRequest
    {
  
        public string? CardNumber { get; set; }
        public decimal Amount { get; set; }
        public string IPAddress { get; set; }
    }
