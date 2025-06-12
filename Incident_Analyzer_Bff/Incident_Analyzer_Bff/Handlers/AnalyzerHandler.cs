using Incident_Analyzer_Bff.Models;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using Amazon.Bedrock;
using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon;
using System.Text;
using Incident_Analyzer_Bff.Services;
using LLM_app;


namespace Incident_Analyzer_Bff.Handler
{
    public class AnalyzerHandler
    {

        public async Task<DetailsModel> GetErrorDetails(string cid)
        {
            var prompt = await GenerateErrorPrompt(cid);
            var response = await GetDetailsFromBedrock(prompt);
            return JsonConvert.DeserializeObject<DetailsModel>(response); 
        }


        private async Task<string> GenerateErrorPrompt(string cid)
        {  //get context 
            var accessKey = "AKIARIQUO5RSXXN4J66S";
            var secretKey = "A6Tj8sxgxITkMEbfvx3va1YbF6XcqcCf2lrSzwa1";
            string region = "us-east-1"; // Replace with your region
            string logGroupName = "PaymentApiLogs";

            var cloudwatchService = new CloudWatchService(accessKey, secretKey, region);
            var context = await cloudwatchService.FetchFilteredLogs(logGroupName, cid);

            return $@"
                    You are an intelligent assistant that diagnoses software issues.

                    Given the correlation ID (CID): {cid}, perform the following tasks:

                    1. Search your internal knowledge and trained logs for errors related to this CID.
                    2. Fetch error context from known incident patterns or similar cases.
                    3. If available, fetch relevant details from MCP (Monitoring Control Plane) like:
                        - Stack trace
                        - Root cause
                        - Service/component involved
                        - Suggested fix

                    Output format:
                    - string CID: {cid}
                    - string ErrorSummary:
                    - List<string> AfftectedServivces :
                    - string ErrorInService :
                    - string ErrorMessage :
                    - string RootCause 
                    - string Solution :
                    
                    Here are the CloudWatch logs for the cid = {context}, please analyze them
                    and provide relevant information.
                    If no information is found, state: 'No relevant error data found for CID: {cid}'.
                    
                    Analyze and fetch all the details required in output.
                    Provide the response in json which can be deserialized in the output format provided only. 
                    Json model for output
                    {{
                        ""CID"": ""{cid}"",
                        ""ErrorSummary"": """",
                        ""AffectedServices"": [],
                        ""ErrorInService"": """",
                        ""ErrorMessage"": """",
                        ""RootCause"": """",
                        ""Solution"": """"
                    }}  
                    Do not add extra spaces and do not beautify the json.
                    ";
        }

        private async Task<string> GetDetailsFromBedrock(string prompt)
        {
          
            var config = new AmazonBedrockConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest1, // Specify your region
            };

            BedrockClient bedrockClient = new BedrockClient();
            var response=await bedrockClient.SendPromptAsync(prompt);
            return response;
        }

      
    }
}

