using Nethereum.Siwe.Core;

namespace ExampleProjectSiwe.RestApi.Authorisation;

public interface ISiweJwtAuthorisationService
{
    string GenerateToken(SiweMessage user, string signature);
    Task<SiweMessage> ValidateToken(string token);
}