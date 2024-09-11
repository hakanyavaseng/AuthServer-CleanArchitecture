namespace AuthServer.Application.DTOs.Roles;

public class AssignRoleToUserDto
{
    public Guid UserId { get; set; }
    public List<Guid> RoleIds { get; set; }
}