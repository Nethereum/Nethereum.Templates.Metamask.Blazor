using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;

namespace ExampleProject.Server.Data
{
    public class EthereumAuthenticationService
    {
        public Dictionary<string, ClaimsPrincipal> Users { get; set; }

        public EthereumAuthenticationService()
        {
            Users = new Dictionary<string, ClaimsPrincipal>();
        }
    }

    public class Test<TUser> : RevalidatingServerAuthenticationStateProvider
    {
        public Test(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);
    }
}
