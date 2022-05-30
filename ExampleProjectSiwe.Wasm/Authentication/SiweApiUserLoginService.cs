using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ExampleProjectSiwe.Wasm.Util;
using Nethereum.Siwe.Core;

namespace ExampleProjectSiwe.Wasm.Authentication;

public class SiweApiUserLoginService<TUser> where TUser : User
{
    private readonly HttpClient _httpClient;

    public class AuthenticateRequest
    {
        public string SiweEncodedMessage { get; set; }
        public string Signature { get; set; }
    }

    public class AuthenticateResponse
    {
        public string Address { get; set; }
        public string Jwt { get; set; }
    }

    public SiweApiUserLoginService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateNewSiweMessage(string ethereumAddress)
    {

        var httpMessageResponse = await _httpClient.PostAsync("authentication/newsiwemessage", JsonContent.Create(ethereumAddress));
        var message = await httpMessageResponse.Content.ReadAsStringAsync();
        return message;
    }


    public async Task<AuthenticateResponse> Authenticate(SiweMessage siweMessage, string signature)
    {
        var siweMessageEncoded = SiweMessageStringBuilder.BuildMessage(siweMessage);
        var request = new AuthenticateRequest()
        {
            SiweEncodedMessage = siweMessageEncoded,
            Signature = signature
        };

        var httpMessageResponse = await _httpClient.PostAsJsonAsync("authentication/authenticate", request);

        return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticateResponse>();
    }

    public async Task<TUser> GetUser(string token)
    {
        try
        {
            var user = await _httpClient.GetAsync<TUser>("authentication/getuser", token);
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetType());
            return null;
        }
    }

    public async Task Logout(string token)
    {
        try
        {
            var httpResponse = await _httpClient.PostAsync("authentication/logout", null, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetType());
            throw;
        }
    }

}