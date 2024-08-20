namespace AuthServer.Application.DTOs.Auth
{
    public class ActionDto
    {
        public string ActionType { get; set; }
        public string HttpType { get; set; }
        public string Definition { get; set; }
        public string Code { get; set; }
    }
}
