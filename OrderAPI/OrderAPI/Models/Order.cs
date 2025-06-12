
using OrderAPI.Models;
using System;
using System.Collections.Generic;

public class Order
{
    public string OrderId { get; set; }
    public decimal Price { get; set; }
    public string OrderStatus { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime LastUpdatedOn { get; set; }
    public List<Item> Items { get; set; }
    public string CustomerName { get; set; }
}
