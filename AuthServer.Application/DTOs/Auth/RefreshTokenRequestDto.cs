namespace AuthServer.Application.DTOs.Auth
{
    public record RefreshTokenRequestDto
    {
        public string UsernameOrEmail { get; init; }
        public string RefreshToken { get; init; }
    }
}
