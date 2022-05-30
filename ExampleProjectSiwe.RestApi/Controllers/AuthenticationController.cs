using ExampleProjectSiwe.RestApi.Authorisation;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Siwe;
using Nethereum.Siwe.Core;
using Nethereum.Util;

namespace ExampleProjectSiwe.RestApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly ISiweJwtAuthorisationService _siweJwtAuthorisationService;
        private readonly SiweMessageService _siweMessageService;

        public AuthenticationController(SiweMessageService siweMessageService, ISiweJwtAuthorisationService siweJwtAuthorisationService)
        {
            _siweMessageService = siweMessageService;
            _siweJwtAuthorisationService = siweJwtAuthorisationService;
        }

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

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest authenticateRequest)
        {
            var siweMessage = SiweMessageParser.Parse(authenticateRequest.SiweEncodedMessage);
            var signature = authenticateRequest.Signature;
            var validUser = await _siweMessageService.IsUserAddressRegistered(siweMessage);
            if (validUser)
            {
                if (await _siweMessageService.IsMessageSignatureValid(siweMessage, signature))
                {
                    if (_siweMessageService.IsMessageTheSameAsSessionStored(siweMessage))
                    {
                        if (_siweMessageService.HasMessageDateStartedAndNotExpired(siweMessage))
                        {
                            var token = _siweJwtAuthorisationService.GenerateToken(siweMessage, signature);
                            return Ok(new AuthenticateResponse
                            {
                                Address = siweMessage.Address,
                                Jwt = token
                            });
                        }
                        ModelState.AddModelError("Unauthorized", "Expired token");
                        return Unauthorized(ModelState);
                    }
                    ModelState.AddModelError("Unauthorized", "Matching Siwe message with nonce not found");
                    return Unauthorized(ModelState);
                }
                ModelState.AddModelError("Unauthorized", "Invalid Signature");
                return Unauthorized(ModelState);
            }

            ModelState.AddModelError("Unauthorized", "Invalid User");
            return Unauthorized(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("newsiwemessage")]
        public IActionResult GenerateNewSiweMessage([FromBody] string address)
        {
            var message = new DefaultSiweMessage();
            message.SetExpirationTime(DateTime.Now.AddMinutes(10));
            message.SetNotBefore(DateTime.Now);
            message.Address = address.ConvertToEthereumChecksumAddress();
            return Ok(_siweMessageService.BuildMessageToSign(message));
        }


        [HttpPost("logout")]
        public IActionResult LogOut()
        {
            var siweMessage = SiweJwtMiddleware.GetSiweMessageFromContext(HttpContext);
            _siweMessageService.InvalidateSession(siweMessage);
            return Ok();
        }

        [HttpGet("getuser")]
        public IActionResult GetAuthenticatedUser()
        {
            //ethereum address
            var address = SiweJwtMiddleware.GetEthereumAddressFromContext(HttpContext);
            if (address != null)
            {
                //Get all user details .. from a service etc 
                return Ok(new User { EthereumAddress = address });
            }
            //this should not happen
            return Forbid();
        }

        public class User
        {
            public string EthereumAddress { get; set; }
            public string UserName { get; set; } = "Vitalik";
            public string Email { get; set; } = "test@ExampleProject.com";
        }
    }




}
