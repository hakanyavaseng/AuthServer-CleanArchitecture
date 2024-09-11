using Microsoft.AspNetCore.Identity;

namespace AuthServer.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public ICollection<AuthEndpoint> AuthEndpoints { get; set; }
}