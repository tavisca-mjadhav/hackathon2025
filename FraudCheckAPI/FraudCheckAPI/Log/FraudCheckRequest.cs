using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace FraudCheckAPI.Log
{
    public class FraudCheckRequest
    {
        [BindNever]
        public string? CardNumber { get; set; }
        public decimal Amount { get; set; }
        public string IPAddress { get; set; }
    }
}
