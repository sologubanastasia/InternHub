using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System; // Додано для Guid

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
            
            // Якщо заголовок userId порожній, повертаємо Failure, що призведе до 401 Unauthorized
            if (string.IsNullOrEmpty(userIdString))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication header missing or empty."));
            }

            if (string.IsNullOrEmpty(role))
                role = RoleConstants.Candidate; // Припускаємо, що RoleConstants визначений

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userIdString),
                new Claim("UserId", userIdString), // ДОДАНО для контролерів, які шукають "UserId"
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