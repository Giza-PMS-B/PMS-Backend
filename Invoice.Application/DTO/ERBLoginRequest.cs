using System;

namespace Invoice.Application.DTO;

public class ERBLoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
