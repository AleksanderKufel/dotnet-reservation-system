using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests
{
    public static class TestAuthHelper
    {
        public static async Task AuthenticateAsync(
            HttpClient client)
        {
            var email =
                $"test-{Guid.NewGuid()}@test.com";

            const string password = "String-1";

            var registerResponse =
                await client.PostAsJsonAsync(
                    "/register",
                    new
                    {
                        email,
                        password
                    });

            registerResponse.EnsureSuccessStatusCode();

            var loginResponse =
                await client.PostAsJsonAsync(
                    "/login",
                    new
                    {
                        email,
                        password
                    });

            loginResponse.EnsureSuccessStatusCode();

            var token =
                await loginResponse.Content
                    .ReadFromJsonAsync<LoginResponse>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token!.AccessToken);
        }
    }
}
