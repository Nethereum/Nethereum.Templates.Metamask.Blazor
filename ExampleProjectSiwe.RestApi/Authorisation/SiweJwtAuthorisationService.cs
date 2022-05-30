using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Siwe;
using Nethereum.Siwe.Core;

namespace ExampleProjectSiwe.RestApi.Authorisation
{
    public class SiweJwtAuthorisationService : ISiweJwtAuthorisationService
    {
        private readonly SiweMessageService _siweMessageService;

        private readonly AppSettings _appSettings;
        private const string ClaimTypeAddress = "address";
        private const string ClaimTypeNonce = "nonce";
        private const string ClaimTypeSignature = "signature";
        private const string ClaimTypeSiweExpiry = "siweExpiry";
        private const string ClaimTypeSiweIssuedAt = "siweIssueAt";
        private const string ClaimTypeSiweNotBefore = "siweNotBefore";

        public SiweJwtAuthorisationService(IOptions<AppSettings> appSettings, SiweMessageService siweMessageService)
        {
            _siweMessageService = siweMessageService;
            _appSettings = appSettings.Value;
        }

        public string GenerateToken(SiweMessage siweMessage, string signature)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                                                     new Claim(ClaimTypeAddress, siweMessage.Address) ,
                                                     new Claim(ClaimTypeNonce, siweMessage.Nonce),
                                                     new Claim(ClaimTypeSignature, signature),
                                                     new Claim(ClaimTypeSiweExpiry, siweMessage.ExpirationTime),
                                                     new Claim(ClaimTypeSiweIssuedAt, siweMessage.IssuedAt),
                                                     new Claim(ClaimTypeSiweNotBefore, siweMessage.NotBefore),
                                            }),


                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            if (!string.IsNullOrEmpty(siweMessage.ExpirationTime))
            {
                tokenDescriptor.Expires = GetIso8602AsDateTime(siweMessage.ExpirationTime);
            }
            if (!string.IsNullOrEmpty(siweMessage.IssuedAt))
            {
                tokenDescriptor.IssuedAt = GetIso8602AsDateTime(siweMessage.IssuedAt);

            }
            if (!string.IsNullOrEmpty(siweMessage.NotBefore))
            {
                tokenDescriptor.NotBefore = GetIso8602AsDateTime(siweMessage.NotBefore);
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        protected DateTime GetIso8602AsDateTime(string iso8601dateTime)
        {
            return DateTime.ParseExact(iso8601dateTime, "o",
                System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();
        }

        public async Task<SiweMessage> ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var address = jwtToken.Claims.First(x => x.Type == ClaimTypeAddress).Value;
                var nonce = jwtToken.Claims.First(x => x.Type == ClaimTypeNonce).Value;
                var issuedAt = jwtToken.Claims.First(x => x.Type == ClaimTypeSiweIssuedAt).Value;
                var expiry = jwtToken.Claims.First(x => x.Type == ClaimTypeSiweExpiry).Value;
                var notBefore = jwtToken.Claims.First(x => x.Type == ClaimTypeSiweNotBefore).Value;



                var signature = jwtToken.Claims.First(x => x.Type == ClaimTypeSignature).Value;



                var siweMessage = new DefaultSiweMessage
                {
                    Address = address,
                    Nonce = nonce,
                    ExpirationTime = expiry,
                    IssuedAt = issuedAt,
                    NotBefore = notBefore
                };
                //We could use the values stored in the jwt token but if NotBefore or Expiration are not set this will be defaulted
                //and we may not want to expire and renew it
                //also milliseconds are not set in the jwtToken so this causes a validation failure, for this to match milliseconds have to be zero

                //if (jwtToken.IssuedAt != DateTime.MinValue)
                //{
                //    siweMessage.SetIssuedAt(jwtToken.IssuedAt);
                //}

                //if (jwtToken.ValidFrom != DateTime.MinValue)
                //{
                //    siweMessage.SetNotBefore(jwtToken.ValidFrom);
                //}

                //if (jwtToken.ValidTo != DateTime.MinValue)
                //{
                //    siweMessage.SetExpirationTime(jwtToken.ValidTo);
                //}

                Debug.WriteLine(SiweMessageStringBuilder.BuildMessage(siweMessage));
                if (await _siweMessageService.IsMessageSignatureValid(siweMessage, signature))
                {
                    if (_siweMessageService.IsMessageTheSameAsSessionStored(siweMessage))
                    {
                        if (_siweMessageService.HasMessageDateStartedAndNotExpired(siweMessage))
                        {
                            return siweMessage;
                        }

                    }
                }

                return null;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}
