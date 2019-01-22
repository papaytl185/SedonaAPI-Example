using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace SOUC2019
{
    class Program
    {
        private static Uri sedonaCloudUri = new Uri("http://sedonacloud.local:8082");

        static void Main(string[] args)
        {
            var httpClient = new HttpClient();

            getCustomers(httpClient);

            Console.ReadLine();
        }

        private async static void getCustomers(HttpClient client)
        {
            // obtain the discovery document
            var discoveryDoc = await getDiscoveryDoc(client);
            if (discoveryDoc.IsError)
                return;

            // lets get an accesstoken
            var requestTokeResponse = await requestTokenPasswordFlowAsync(client, discoveryDoc);
            if (requestTokeResponse.IsError)
                return;

            // get customers
            await getCustomerAPI(client, requestTokeResponse);
        }

        private async static Task getCustomerAPI(HttpClient client, TokenResponse requestTokeResponse)
        {
            client.SetBearerToken(requestTokeResponse.AccessToken);

            var response = await client.GetAsync($"{sedonaCloudUri.AbsoluteUri}api/customer?$select=CustomerNumber,CustomerName");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
            else
            {
                Console.WriteLine(response.StatusCode);
            }
        }

        private async static Task<TokenResponse> requestTokenPasswordFlowAsync(HttpClient client, DiscoveryResponse discoveryDoc)
        {
            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,
                ClientId = "ro.SOUC2019",
                ClientSecret = "MarcoIsland!",

                UserName = "SOUC",
                Password = "Souc2019!",

                Scope = "sedonacloudapi sedonacloudscope offline_access openid",
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
            }
            else
            {
                Console.WriteLine(tokenResponse.AccessToken);
            }

            return tokenResponse;
        }

        private async static Task<DiscoveryResponse> getDiscoveryDoc(HttpClient client)
        {
            var discoveryClient = new DiscoveryClient(sedonaCloudUri.AbsoluteUri);
            discoveryClient.Policy.RequireHttps = false;

            var disco = await discoveryClient.GetAsync();
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
            }
            return disco;
        }
    }
}
