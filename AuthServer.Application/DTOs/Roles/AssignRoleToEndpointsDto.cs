namespace AuthServer.Application.DTOs.Roles;

public class AssignRoleToEndpointsDto
{
    public string[] Roles { get; set; }
    public List<string> EndpointCodes { get; set; }
}