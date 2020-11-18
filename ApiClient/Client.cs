using System.Net.Http;
using System.Net.Http.Headers;

namespace ApiClient
{
    internal class Client
    {
        internal HttpClient ApiClient { get; set; }

        internal Client()
        {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/javascript")); // That's what DreamHost's API gives as content type ¯\_(ツ)_/¯
        }
    }
}
