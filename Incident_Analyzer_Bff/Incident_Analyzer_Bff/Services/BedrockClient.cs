﻿using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Runtime;
using static System.Net.Mime.MediaTypeNames;

namespace Incident_Analyzer_Bff.Services
{
    public class BedrockClient
    {
        private readonly AmazonBedrockRuntimeClient _bedrockClient;
        public BedrockClient()
        {
            HttpService httpService = new HttpService(new HttpClient());
            var credentials = httpService.GetSecrets().GetAwaiter().GetResult();
            var accessKey = credentials.KeyId;
            var secretKey = credentials.SecretKey;
            var creds = new BasicAWSCredentials(accessKey, secretKey);

            var config = new AmazonBedrockRuntimeConfig
            {
                RegionEndpoint = RegionEndpoint.USEast1
            };

            _bedrockClient = new AmazonBedrockRuntimeClient(creds, config);
        }

        public async Task<string> SendPromptAsync(string prompt)
        {
            var modelId = "arn:aws:bedrock:us-east-1::inference-profile/us.anthropic.claude-3-5-sonnet-20241022-v2:0";
            var requestBody = new
            {
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = 200000,
                temperature = 0.5,
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
                Console.WriteLine("Error! Exception - " + e);
                throw;
            }

            return response.ToString();
        }

        public async Task<string> ConverseAsync(string prompt)
        {
            var modelId = "arn:aws:bedrock:us-east-1::inference-profile/us.anthropic.claude-3-5-sonnet-20241022-v2:0";

            var request = new ConverseStreamRequest
            {
                
                ModelId = modelId,                
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = ConversationRole.User,
                        Content = new List<ContentBlock> { new ContentBlock { Text = prompt } }
                    }
                },
                InferenceConfig = new InferenceConfiguration()
                {
                    MaxTokens = 2048,
                    Temperature = 0.7F,
                    TopP=1.0f                    
                }
            };
            StringBuilder response = new StringBuilder();
            try
            {
                var converseResponse = await _bedrockClient.ConverseStreamAsync(request);

                foreach (var chunk in converseResponse.Stream.AsEnumerable())
                {
                    if (chunk is ContentBlockDeltaEvent)
                    {
                        response.Append((chunk as ContentBlockDeltaEvent).Delta.Text ?? string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: Can't invoke '{modelId}'. Reason: {e.Message}");
                throw;
            }
            return response.ToString();
        }
    }
}
