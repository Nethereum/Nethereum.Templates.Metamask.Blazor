using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExampleProjectSiwe.Wasm.Util
{
    //This could be replaced with ReFit https://github.com/reactiveui/refit
    //https://github.com/reactiveui/refit#bearer-authentication
    public static class HttpClientExtensions
    {
        public static async Task<T> GetAsync<T>(this HttpClient httpClient,
            string url,
            string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(responseBody, jsonSerializerOptions);
        }

        public static async Task<T> PostAsync<T>(this HttpClient httpClient,
            string url,
            object data,
            string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); ;

            if (data != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");
            }

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(responseBody, jsonSerializerOptions);
        }

        public static async Task<HttpResponseMessage> PostAsync(this HttpClient httpClient,
            string url,
            object data,
            string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); ;

            if (data != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");
            }

            return await httpClient.SendAsync(request);

        }

        public static async Task<T> PutAsync<T>(this HttpClient httpClient,
            string url,
            object data,
            string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); ;

            request.Content = new StringContent(JsonSerializer.Serialize(data),
               Encoding.UTF8,
               "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(responseBody, jsonSerializerOptions);
        }

        public static async Task<int> DeleteAsync(this HttpClient httpClient,
            string url,
            string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); ;

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<int>(responseBody, jsonSerializerOptions);
        }
    }
}
