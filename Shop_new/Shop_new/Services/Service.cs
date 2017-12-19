using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
//using System.Threading.Tasks;

namespace Shop_new.Services
{
    public class Service
    {
        private string baseAddress;
        private string appId = "AppId";
        private string appSecret = "AppSecret";
        private string token;

        public Service(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        protected async Task<HttpResponseMessage> PostJson(string addr, object obj)
        {
            using (var client = new HttpClient())
                try
                {
                    //var q = JsonConvert.SerializeObject(obj);
                    // var z = new StringContent(q, Encoding.UTF8, "application/json");
                    await EstablishConnection(client);
                    var result = await client.PostAsync(GetAddress(addr), new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
                    return result.StatusCode != System.Net.HttpStatusCode.Forbidden ? result : null;
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
                    var result = await client.GetAsync(GetAddress(addr));
                    return result.StatusCode != System.Net.HttpStatusCode.Forbidden ? result : null;
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
                    var result = await client.DeleteAsync(GetAddress(addr));
                    return result.StatusCode != System.Net.HttpStatusCode.Forbidden ? result : null;
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync(GetAddress("/"));
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    token = await GetToken(appId, appSecret);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }
        }

        private async Task<string> GetToken(string id, string secret)
        {
            HttpResponseMessage result = null;
            using (var client = new HttpClient())
                try
                {
                    var str = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{id}:{secret}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", str);
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
