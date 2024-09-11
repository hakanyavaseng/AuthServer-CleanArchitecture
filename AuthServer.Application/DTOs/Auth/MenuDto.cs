namespace AuthServer.Application.DTOs.Auth;

public class MenuDto
{
    public string Name { get; set; }
    public List<ActionDto> Actions { get; set; } = new();
}