using Incident_Analyzer_Bff.Models;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using Amazon.Bedrock;
using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon;
using System.Text;
using Incident_Analyzer_Bff.Services;


namespace Incident_Analyzer_Bff.Handler
{
    public class AnalyzerHandler
    {

        public async Task<string> GetErrorDetails(string cid)
        {
            var prompt = GenerateErrorPrompt(cid);
            var response = await GetDetailsFromBedrock(prompt);
            return response.ToString(); 
        }


        private static string GenerateErrorPrompt(string cid)
        {
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
                    - CID: {cid}
                    - Error Summary:
                    - Affected Service/Module:
                    - Root Cause (if known):
                    - Suggested Fix/Next Steps:

                    If no information is found, state: 'No relevant error data found for CID: {cid}'.

                    Provide the response in plain English.
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

