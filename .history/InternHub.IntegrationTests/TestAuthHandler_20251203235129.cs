using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace InternHub.IntegrationTests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) 
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userIdString = Request.Headers["X-Test-UserId"].ToString();
            var role = Request.Headers["X-Test-Role"].ToString();
            
            if (string.IsNullOrEmpty(userIdString))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication header missing or empty."));
            }

            if (string.IsNullOrEmpty(role))
                role = "Candidate"; 

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userIdString),
                new Claim("UserId", userIdString),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}