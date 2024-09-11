namespace AuthServer.Application.DTOs.Auth;

public record UserRefreshToken
{
    public Guid UserId { get; set; }
    public string Code { get; set; }
    public DateTime ExpirationTime { get; set; }
}