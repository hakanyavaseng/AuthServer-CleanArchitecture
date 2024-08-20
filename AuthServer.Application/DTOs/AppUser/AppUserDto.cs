namespace AuthServer.Application.DTOs.AppUser
{
    public record AppUserDto
    {
        public Guid Id { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
    }
}
