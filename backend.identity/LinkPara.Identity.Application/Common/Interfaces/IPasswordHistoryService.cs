using LinkPara.Identity.Domain.Entities;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IPasswordHistoryService
{
    Task<List<UserPasswordHistory>> GetOldPasswordsAsync(User user);
    Task SavePasswordAsync(User user, string oldHashedPassword);
}