namespace AuthServer.Application.DTOs.Auth
{
    public record LoginDto
    {
        public string UserNameOrEmail { get; init; }
        public string Password { get; init; }
    }
}
