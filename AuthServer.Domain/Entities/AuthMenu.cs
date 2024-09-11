using AuthServer.Domain.Entities.Common;

namespace AuthServer.Domain.Entities;

public class AuthMenu : CreationAuditedEntity<Guid>
{
    public string Name { get; set; }
    public ICollection<AuthEndpoint> AuthEndpoints { get; set; }
}