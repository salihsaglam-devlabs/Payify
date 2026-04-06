using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.SystemUser;

public class ApplicationUserService : IApplicationUserService
{
    private readonly IUserService _userService;
    public Guid ApplicationUserId { get;  set; }
    public string Token { get; set; }
    public ApplicationUserService(IUserService userService)
    {
        _userService = userService;
    }
    public async Task<Guid> ConfigureApplicationUserAsync(string username ,string password)
    {
        var email = username.ToLowerInvariant() + "@" + username.ToLowerInvariant() + ".com";
        var request = new GetAppUsersRequest { UserName = username };
        var users = await _userService.GetApplicationUserAsync(request);

        var userId = new Guid();
        if (users.Count > 0)
        {
            var existAppUser = users.First();
            userId = existAppUser.Id;
        }
        else
        {
            var applicationUser = new CreateUserRequest
            {
                Email = email,
                PhoneCode = "App",
                FirstName = username,
                LastName = username,
                PhoneNumber = username,
                UserType = UserType.ApplicationUser,
                Password = password,
                UserName = username

            };
            var userResponse = await _userService.CreateUserAsync(applicationUser);
            userId = userResponse.UserId;
        }
        return userId;
    }
    public async Task<string> ConfigureApplicationLoginAsync(string username, string password)
    {
        var login = new LoginRequest
        {
            Username = username,
            Password = password,
            RememberMe = false
        };
        var loginResponse = await _userService.LoginAsync(login);
        var token = loginResponse.AccessToken;

        return token ?? "";
    }

}