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
using System.Security.Cryptography;


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
            //string logGroupName = "LogAnalysis";

            var cloudwatchService = new CloudWatchService(accessKey, secretKey, region);
            var context = await cloudwatchService.FetchFilteredLogs(logGroupName, cid);
            var contactGenerator = new ContactGenerator();
            var contactDetails = contactGenerator.GenerateRandomContacts(2);

            return $@"
                    You are an intelligent assistant that diagnoses software issues.

                    Given the correlation ID (CID): {cid}, perform the following tasks:

                    1. Search your internal knowledge and trained logs for errors related to this CID.
                    2. Fetch error context from known incident patterns or similar cases.
                    3. Analyze the CloudWatch logs for the CID and extract relevant information and also extract below details:
        
                    - string CID: {cid}
                    - string ErrorSummary:
                    - string AfftectedServivces :  add names of all services where the current cid is found.
                    - string ErrorInService :
                    - string ErrorMessage :
                    - string RootCause 
                    - string Solution :
                    - string impact;
                    - string ContactDetails : add {contactDetails} here  
                    
                    Here are the CloudWatch logs for the cid = {context}, please analyze them
                    and provide relevant information.
                    If no information is found, state: 'No relevant error data found for CID: {cid}'.
                    
                    Analyze and fetch all the details required in output.
                    Also provide the possible impact of this issue on our buisness in terms of numbers. 
                    Provide the response in json which can be deserialized in the output format provided only. 
                    Json model for output
                    {{
                        ""CID"": ""{cid}"",
                        ""ErrorSummary"": """",
                        ""AffectedServices"": """",
                        ""ErrorInService"": """",
                        ""ErrorMessage"": """",
                        ""RootCause"": """",
                        ""Solution"": """",
                        ""Impact"": """",
                        ""ContactDetails"": """",
                    }}  
                    Do not add extra spaces and do not beautify the json. But just new numeric point in each on new line.
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

      
        public async Task<string> ConverseWithBedrock(string cid=default,string inputPrompt=default)
        {
            if (string.IsNullOrEmpty(cid) && string.IsNullOrEmpty(inputPrompt))
                return default;
            var prompt = inputPrompt;
            if(!string.IsNullOrEmpty(cid))
             prompt = await GenerateErrorPrompt(cid);

            var config = new AmazonBedrockConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.USWest1, // Specify your region
            };

            BedrockClient bedrockClient = new BedrockClient();
            var response=await bedrockClient.ConverseAsync(prompt);
            return response;
        }

      
    }
}

