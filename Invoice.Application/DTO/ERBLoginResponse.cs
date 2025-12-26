using System;

namespace Invoice.Application.DTO;

public class ERBLoginResponse
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public string TokenType { get; set; } = default!;
    public string Username { get; set; } = default!;
}
