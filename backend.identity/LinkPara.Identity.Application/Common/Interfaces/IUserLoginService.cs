using LinkPara.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IUserLoginService
{
    Task SaveLoginInfoAsync(User user, SignInResult result);
}