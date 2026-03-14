using System.Net.Http.Headers;
using MPStore.Admin.Services;

namespace MPStore.Admin.Helpers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AuthService _authService;

        public AuthHeaderHandler(AuthService authService)
        {
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _authService.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}