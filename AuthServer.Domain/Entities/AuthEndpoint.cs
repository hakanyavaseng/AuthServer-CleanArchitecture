using AuthServer.Domain.Entities.Common;

namespace AuthServer.Domain.Entities;

public class AuthEndpoint : ModificationAuditedEntity<Guid>
{
    public string ActionType { get; set; }
    public string HttpType { get; set; }
    public string Definition { get; set; }
    public string Code { get; set; }
    public AuthMenu Menu { get; set; }
    public ICollection<AppRole> Roles { get; set; }
}