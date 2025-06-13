namespace OrderAPI.Models
{
    public class OrderRequest
    {
        public decimal Price { get; set; }
        public List<ItemRequest> Items { get; set; }
        public string CustomerName { get; set; }
        public string Currency { get; set; }
        public Card Card { get; set; }
    }

    public class ItemRequest
    {
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public decimal ItemPrice { get; set; }

    }
}
