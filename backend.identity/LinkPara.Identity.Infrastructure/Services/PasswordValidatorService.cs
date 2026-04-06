using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace LinkPara.Identity.Infrastructure.Services;

public class PasswordValidatorService : IPasswordValidator<User>
{
    private readonly IConfiguration _configuration;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IVaultClient _vaultClient;

    public PasswordValidatorService(IConfiguration configuration,
        IPasswordHistoryService passwordHistoryService,
        IVaultClient vaultClient)
    {
        _configuration = configuration;
        _passwordHistoryService = passwordHistoryService;
        _vaultClient = vaultClient;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<User> userManager, User user, string password)
    {
        var requiredLength = _vaultClient.GetSecretValue<int>("IdentitySecrets", "PasswordSettings", "RequiredLength");
        if (password.Length != requiredLength)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordLengthError",
                    Description = $"Password must have {requiredLength} characters."
                });
        }

        if (!int.TryParse(password, out _))
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordContentError",
                    Description = $"Password must contains only numbers."
                });
        }

        var repetitiveRegexPattern = _configuration.GetValue<string>("PasswordValidation:RepetitiveRegexPattern");
        if (Regex.IsMatch(password, repetitiveRegexPattern))
        {
            return IdentityResult.Failed(
                  new IdentityError
                  {
                      Code = "PasswordRepetitiveCharacterError",
                      Description = $"Password can not have repetitive characters."
                  });
        }

        var successiveRegexPattern = _configuration.GetValue<string>("PasswordValidation:SuccessiveRegexPattern");
        if (Regex.IsMatch(password, successiveRegexPattern))
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordSuccessiveCharacterError",
                    Description = $"Password can not have successive characters."
                });
        }

        if (!(await ValidatePasswordHistoryAsync(userManager, user, password)))
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Code = "PasswordHistoryRequirementError",
                    Description = $"New password must be different then the old passwords."
                });
        }

        try
        {
            var canStartWithZero = _vaultClient.GetSecretValue<bool>("IdentitySecrets", "PasswordSettings", "CanStartWithZero");
            if (!canStartWithZero && password.StartsWith('0'))
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Code = "PasswordStartsWithZeroError",
                        Description = $"Password can not start with zero."
                    });
            }
        }
        catch (System.Exception)
        { }

        try
        {
            var canHasFourOrMoreSameCharacter = _vaultClient.GetSecretValue<bool>("IdentitySecrets", "PasswordSettings", "CanHasFourOrMoreSameCharacter");
            if (!canHasFourOrMoreSameCharacter && hasFourOrMoreSameCharacter(password))
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Code = "PasswordFourOrMoreSameCharacterError",
                        Description = $"Password can not have four or more same characters."
                    });
            }
        }
        catch (System.Exception)
        { }



        return IdentityResult.Success;
    }

    private async Task<bool> ValidatePasswordHistoryAsync(UserManager<User> userManager, User user, string password)
    {
        var oldPasswords = await _passwordHistoryService.GetOldPasswordsAsync(user);

        foreach (var oldPassword in oldPasswords)
        {
            var oldPasswordVerifiedResult = userManager.PasswordHasher.VerifyHashedPassword(user, oldPassword.PasswordHash, password);

            if (oldPasswordVerifiedResult is not PasswordVerificationResult.Failed)
            {
                return false;
            }
        }
        return true;
    }

    private bool hasFourOrMoreSameCharacter(string password)
    {
        var characterCount = new Dictionary<char, int>();

        foreach (var character in password)
        {
            if (characterCount.ContainsKey(character))
            {
                characterCount[character]++;
            }
            else
            {
                characterCount[character] = 1;
            }

            if (characterCount[character] >= 4)
            {
                return true;
            }
        }

        return false;
    }
}