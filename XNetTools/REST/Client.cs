using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNetTools.REST
{
    public class Client
    {
        HttpClient connector = new HttpClient();
        string BaseURL;

        public Client(string baseURL) {
            BaseURL = baseURL;
        }

        public async Task<string> GETAsString(string url)
        {
            string combinedUrl = $"{BaseURL}/{url}";

            HttpResponseMessage response = await connector.GetAsync(combinedUrl);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                throw new Exception("No response!");
            }

            return content;
        }

        public async Task<Stream> GETAsStream(string url)
        {
            string combinedUrl = $"{BaseURL}/{url}";

            HttpResponseMessage response = await connector.GetAsync(combinedUrl);

            response.EnsureSuccessStatusCode();

            Stream content = await response.Content.ReadAsStreamAsync();

            return content;
        }
    }
}
