using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Identity;


namespace LinkPara.Identity.Infrastructure.Services
{
    public class AppUserTokenService : IAppUserTokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IJwtHelper _jwtHelper;
        private readonly IAuditLogService _auditLogService;
        private readonly IVaultClient _vaultClient;
        public AppUserTokenService(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IApplicationUserService applicationUserService,
            IJwtHelper jwtHelper,
            IVaultClient vaultClient,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationUserService = applicationUserService;
            _jwtHelper = jwtHelper;
            _vaultClient = vaultClient;
            _auditLogService = auditLogService;
        }


        public async Task<string> GetAppUserJwtTokenAsync()
        {
            var username = _vaultClient
                    .GetSecretValue<string>("IdentitySecrets", "AppUser", "Username");

            var user = await _userManager.FindByNameAsync(username);

            if (user is null)
            {
                user = await CreateAppUserAsync(username);
            }

            _applicationUserService.ApplicationUserId = user.Id;

            return await _jwtHelper.GenerateJwtTokenAsync(user);
        }
        private async Task<User> CreateAppUserAsync(string username)
        {

                var password = _vaultClient
                    .GetSecretValue<string>("IdentitySecrets", "AppUser", "Password");
                var email = username.ToLower() + "@" + username.ToLower() + ".com";

                var applicationUser = new User
                {
                    Email = email,
                    PhoneCode = "App",
                    FirstName = username,
                    LastName = username,
                    PhoneNumber = username,
                    UserType = UserType.ApplicationUser,
                    UserName = username,
                    UserStatus = UserStatus.Active,
                    PasswordModifiedDate = DateTime.Now,
                    RecordStatus = RecordStatus.Active,
                    CreateDate = DateTime.Now,
                };

                applicationUser.CreatedBy = applicationUser.Id.ToString();

                var result = await _userManager.CreateAsync(applicationUser, password);

                if (!result.Succeeded)
                {
                    await _auditLogService.AuditLogAsync(
                        new AuditLog
                        {
                            IsSuccess = false,
                            LogDate = DateTime.Now,
                            Operation = "CreateApplicationUser",
                            SourceApplication = "Identity",
                            Resource = "User",
                            UserId = applicationUser.Id,
                            Details = new Dictionary<string, string>
                            {
                        {"UserName", applicationUser.UserName },
                        {"Email", applicationUser.Email },
                        {"ErrorMessage" , result.Errors?.FirstOrDefault()?.Description}
                            }
                        });

                    throw new InvalidRegisterException();
                }

                var role = await _roleManager.FindByNameAsync(applicationUser.UserName);

                if (role == null)
                {
                    role = new Role
                    {
                        Name = applicationUser.UserName,
                        RoleScope = RoleScope.Application,
                        RecordStatus = RecordStatus.Active,
                        CreateDate = DateTime.Now,
                        CreatedBy = applicationUser.Id.ToString()
                    };

                    await _roleManager.CreateAsync(role);
                    Console.WriteLine("AppUser -> Role Created");
                }

                var roleResult = await _userManager.AddToRoleAsync(applicationUser, role?.Name);

                if (!roleResult.Succeeded)
                {
                    await _auditLogService.AuditLogAsync(
                        new AuditLog
                        {
                            IsSuccess = false,
                            LogDate = DateTime.Now,
                            Operation = "AddUserRole",
                            SourceApplication = "Identity",
                            Resource = "UserRole",
                            UserId = applicationUser.Id,
                            Details = new Dictionary<string, string>
                            {
                        {"UserId", applicationUser.Id.ToString() },
                        {"UserName", applicationUser.UserName },
                        {"Email", applicationUser.Email },
                        {"ErrorMessage" , roleResult.Errors?.FirstOrDefault()?.Description}
                            }
                        });

                    throw new InvalidRegisterException();
                }


                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "CreateApplicationUser",
                        SourceApplication = "Identity",
                        Resource = "User",
                        UserId = applicationUser.Id,
                        Details = new Dictionary<string, string>
                        {
                    {"UserId", applicationUser.Id.ToString() },
                    {"UserName", applicationUser.UserName },
                    {"Email", applicationUser.Email },
                        }
                    });

                return applicationUser;
        }
    }
}
