using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationSystem.IntegrationTests
{
    public sealed class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public sealed class LoginResponse
    {
        public string TokenType { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = default!;
    }
}
