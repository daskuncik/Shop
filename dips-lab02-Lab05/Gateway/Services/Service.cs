using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Services
{
    public abstract class Service
    {
        private string baseAddress;
        private string appId;
        private string appSecret;
        private string token;

        public Service(string baseAddress, string appId = "AppId", string appSecret = "AppSecret")
        {
            this.baseAddress = baseAddress;
            this.appId = appId;
            this.appSecret = appSecret;
        }

        protected async Task<HttpResponseMessage> PostJson(string addr, object obj)
        {
    //        var handler = new HttpClientHandler
    //        {
    //            ClientCertificateOptions = ClientCertificateOption.Manual,
    //            ServerCertificateCustomValidationCallback =
    //            (httpRequestMessage, cert, cetChain, policyErrors) =>
    //            {
    //                return true;
    //            }
    //        };
    //        ServicePointManager.ServerCertificateValidationCallback +=
    //(sender, cert, chain, sslPolicyErrors) => true;
            // comparable to modern browsers
            using (var client = new HttpClient())
                try
                { 
                    await EstablishConnection(client);
                    return await client.PostAsync(GetAddress(addr), new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> PostForm(string addr, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient())
                try
                {
                    await EstablishConnection(client);
                    return await client.PostAsync(GetAddress(addr), new FormUrlEncodedContent(parameters));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> PutForm(string addr, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient())
                try
                {
                    await EstablishConnection(client);
                    return await client.PutAsync(GetAddress(addr), new FormUrlEncodedContent(parameters));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> Get(string addr)
        {
            using (var client = new HttpClient())
                try
                {
                    await EstablishConnection(client);
                    return await client.GetAsync(GetAddress(addr));
                }
                catch
                {
                    return null;
                }
        }

        protected async Task<HttpResponseMessage> Delete(string addr)
        {
            using (var client = new HttpClient())
                try
                {
                    await EstablishConnection(client);
                    return await client.DeleteAsync(GetAddress(addr));
                }
                catch
                {
                    return null;
                }
        }

        private string GetAddress(string addr)
        {
            return $"{baseAddress}/{addr}";
        }

        private async Task EstablishConnection(HttpClient client)
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync(GetAddress(string.Empty));
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    token = await GetToken(appId, appSecret);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            catch { }
        }

        private async Task<string> GetToken(string id, string secret)
        {
            HttpResponseMessage result = null;
            using (var client = new HttpClient())
                try
                {
                    client.SetBasicAuthentication(appId, appSecret);
                    result = await client.GetAsync(GetAddress(string.Empty));
                }
                catch
                {
                    return null;
                }
            if (result.IsSuccessStatusCode)
                return await result.Content.ReadAsStringAsync();
            else
                return null;
        }
    }
}
