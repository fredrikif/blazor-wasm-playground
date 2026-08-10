using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace blazor_wasm_playground.Services
{
    public class FirebaseAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;

        public FirebaseAuthenticationStateProvider(AuthService authService)
        {
            _authService = authService;
            _authService.AuthStateChanged += NotifyAuthStateChanged;
        }

        private void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _authService.CurrentUser;
            ClaimsPrincipal principal;

            if (user == null)
            {
                principal = new ClaimsPrincipal(new ClaimsIdentity());
            }
            else
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Uid ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.Email ?? string.Empty)
                }, "FirebaseAuth");

                principal = new ClaimsPrincipal(identity);
            }

            return Task.FromResult(new AuthenticationState(principal));
        }
    }
}
