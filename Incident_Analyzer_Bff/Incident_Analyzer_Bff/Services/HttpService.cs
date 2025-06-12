using Amazon;
using Amazon.Runtime;
using Incident_Analyzer_Bff.Models;
using System.Net.Http;

namespace Incident_Analyzer_Bff.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<Credentials> GetSecrets()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Credentials>("https://iincident-sentinel.s3.us-east-1.amazonaws.com/credentials.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }

        
    }
}
