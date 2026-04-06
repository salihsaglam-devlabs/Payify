namespace LinkPara.SystemUser;

public interface IApplicationUserService
{
    Guid ApplicationUserId { get; set; }
    string Token { get; set; }
    Task<Guid> ConfigureApplicationUserAsync(string applicationName, string password);
    Task<string> ConfigureApplicationLoginAsync(string applicationName, string password);
}