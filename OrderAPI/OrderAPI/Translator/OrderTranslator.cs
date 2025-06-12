using OrderAPI.Common;
using OrderAPI.Models;
using System.Runtime.CompilerServices;

namespace OrderAPI.Translator
{
    public static class OrderTranslator
    {
        public static Order ToOrder(this OrderRequest request)
        {
            var order = new Order();
            order.OrderId = CommonUtils.GenerateRandomAlphanumeric();
            order.Price = request.Price;
            order.OrderStatus = Status.Complated.ToString();
            order.CustomerName = request.CustomerName;
            order.Items = request.Items.ToItem();
            order.LastUpdatedOn = DateTime.Now;
            return order; 
        }
        public static List<Item> ToItem(this List<ItemRequest> itemRequests)
        {
            return itemRequests.Select(itemRequest => new Item
            {
                ItemId = CommonUtils.GenerateRandomAlphanumeric(),
                ProductName = itemRequest.ProductName,
                ProductType = itemRequest.ProductType,
                ItemPrice = itemRequest.ItemPrice,
                ItemStatus = Status.Complated.ToString() // Assuming a default status for items
            }).ToList();
        }
    }
}
