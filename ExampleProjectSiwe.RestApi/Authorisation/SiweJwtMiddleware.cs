using Nethereum.Siwe.Core;
using Nethereum.Siwe.UserServices;

namespace ExampleProjectSiwe.RestApi.Authorisation;

public class SiweJwtMiddleware
{
    private readonly RequestDelegate _next;
    public const string ContextEthereumAddress = "ethereumAddress";
    public const string ContextSiweMessage = "siweMessage";

    public static string? GetEthereumAddressFromContext(HttpContext context)
    {
        if (context.Items.ContainsKey(ContextEthereumAddress))
        {
            return (string)context.Items[ContextEthereumAddress]!;
        }

        return null;
    }

    public static SiweMessage? GetSiweMessageFromContext(HttpContext context)
    {
        if (context.Items.ContainsKey(ContextSiweMessage))
        {
            return (SiweMessage)context.Items[ContextSiweMessage]!;
        }

        return null;
    }

    public static void SetSiweMessage(HttpContext context, SiweMessage siweMessage)
    {
        context.Items[ContextSiweMessage] = siweMessage;
    }

    public static void SetEthereumAddress(HttpContext context, string address)
    {
        context.Items[ContextEthereumAddress] = address;
    }

    public SiweJwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISiweJwtAuthorisationService siweJwtAuthorisation)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var siweMessage = await siweJwtAuthorisation.ValidateToken(token);
        if (siweMessage != null)
        {
            SetEthereumAddress(context, siweMessage.Address);
            SetSiweMessage(context, siweMessage);
        }

        await _next(context);
    }
}