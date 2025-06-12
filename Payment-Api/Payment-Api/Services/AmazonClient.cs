using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using PaymentApi.Interfaces;
using PaymentApi.Models;
using System.ComponentModel;

namespace OrderAPI.Log
{
    public class AmazonClient
    {
        private readonly IAmazonDynamoDB _dynamoDB;
        private readonly ICloudWatchLogger _cloudWatchLogs;
        public AmazonClient(IAmazonDynamoDB dynamoDB)
        {
            _dynamoDB = dynamoDB;
        }

        public async Task<PutItemResponse> PutItem(Payment payment)
        {
            try
            {
                var item = new Dictionary<string, AttributeValue>
                {
                    ["OrderId"] = new AttributeValue { S = payment.OrderId },
                    ["Id"] = new AttributeValue { S = payment.Id },
                    ["PaymentType"] = new AttributeValue { S = payment.PaymentType.ToString() },
                 
                    ["Amount"] = new AttributeValue { S = payment.Amount.ToString() },
                    ["Currency"] = new AttributeValue { S = payment.Currency.ToString() },
                    ["Amount"] = new AttributeValue { S = payment.Amount.ToString() },
                    ["Amount"] = new AttributeValue { S = payment.Amount.ToString() },


                    ["Card"] = new AttributeValue {  S= JsonConvert.SerializeObject(payment.Card) }
                    // Add other fields as needed
                };
               
                var request = new PutItemRequest
                {
                    TableName = "nextgen_payment",
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

        public async Task<GetItemResponse> GetPayment(string orderId)
        {
            try
            {
                var request = new GetItemRequest
                {
                    TableName = "nextgen_payment",
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
