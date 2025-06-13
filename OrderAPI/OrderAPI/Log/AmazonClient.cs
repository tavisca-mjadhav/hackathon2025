using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System.ComponentModel;

namespace OrderAPI.Log
{
    public class AmazonClient
    {
        private readonly IAmazonDynamoDB _dynamoDB;
        private readonly CloudWatchLogger _cloudWatchLogs;
        public AmazonClient(IAmazonDynamoDB dynamoDB)
        {
            _dynamoDB = dynamoDB;
        }

        public async Task<PutItemResponse> PutItem(Order order)
        {
            try
            {
                var item = new Dictionary<string, AttributeValue>
                {
                    ["OrderId"] = new AttributeValue { S = order.OrderId },
                    ["CustomerName"] = new AttributeValue { S = order.CustomerName },
                    ["Price"] = new AttributeValue { N = order.Price.ToString() },
                    ["LastUpdatedOn"] = new AttributeValue { S = order.LastUpdatedOn.ToString("o") },
                    ["OrderStatus"] = new AttributeValue { S = order.OrderStatus.ToString() },
                    ["Items"] = new AttributeValue { S = JsonConvert.SerializeObject(order.Items) }
                    // Add other fields as needed
                };
                if (order.PaymentStatus != null)
                {
                    item.Add("PaymentStatus", new AttributeValue { S = order.PaymentStatus.ToString() });
                }
                var request = new PutItemRequest
                {
                    TableName = "nextgen_order",
                    Item = item
                };
                var response = await _dynamoDB.PutItemAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                await _cloudWatchLogs.LogErrorAsync("ERROR", ex);
            }
            return null;
        }

        public async Task<GetItemResponse> GetOrder(string orderId)
        {
            try
            {
                var request = new GetItemRequest
                {
                    TableName = "nextgen_order",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "OrderId", new AttributeValue { S = orderId } }
                    }
                };
                var response = await _dynamoDB.GetItemAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                await _cloudWatchLogs.LogErrorAsync("ERROR", ex);
            }
            return null;
        }
    }
}
