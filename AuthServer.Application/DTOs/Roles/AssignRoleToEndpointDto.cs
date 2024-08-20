namespace AuthServer.Application.DTOs.Roles
{
    public record AssignRoleToEndpointDto
    {
        public string Menu { get; set; }
        public string[] Roles { get; set; }
        public string Code { get; set; }
    }
}
