namespace Erp.Api.Auth;

public class JwtSettings
{
    public string Issuer { get; set; } = "erp.local";
    public string Audience { get; set; } = "erp.clients";
    public string SigningKey { get; set; } = "dev-signing-key-change";
    public int ExpiryMinutes { get; set; } = 120;
}