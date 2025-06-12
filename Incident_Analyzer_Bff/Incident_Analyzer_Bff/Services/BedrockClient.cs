using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Runtime;

namespace Incident_Analyzer_Bff.Services
{
    public class BedrockClient
    {
        private readonly AmazonBedrockRuntimeClient _bedrockClient;
        public BedrockClient()
        {
            var accessKey = "AKIARIQUO5RSYSKIORML";
            var secretKey = "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";
            var creds = new BasicAWSCredentials(accessKey, secretKey);

            var config = new AmazonBedrockRuntimeConfig
            {
                RegionEndpoint = RegionEndpoint.USEast1 
            };

            _bedrockClient = new AmazonBedrockRuntimeClient(creds, RegionEndpoint.USEast1);
        }

        public async Task<string> SendPromptAsync(string prompt)
        {
            var modelId = "arn:aws:bedrock:us-east-1::inference-profile/us.anthropic.claude-3-5-sonnet-20241022-v2:0";
            var requestBody = new
            {
                anthropic_version= "bedrock-2023-05-31",
                max_tokens = 200000,
                temperature=0.5,
                messages = new[]
                {
                    new{role="user",content=prompt}
                }
            };

            var request = new InvokeModelWithResponseStreamRequest()
            {
                ModelId = modelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestBody)))
            };
            StringBuilder response = new StringBuilder();
            try
            {
                var streamingResponse = await _bedrockClient.InvokeModelWithResponseStreamAsync(request);
                foreach (var item in streamingResponse.Body)
                {
                    var chunk = JsonSerializer.Deserialize<JsonObject>((item as PayloadPart).Bytes);
                    var text = chunk["delta"]?["text"]?.ToString();
                    Console.WriteLine(text);
                    response.Append(text ?? string.Empty);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error! Exception - "+e);
                throw;
            }

            return response.ToString();
        }
    }
}
