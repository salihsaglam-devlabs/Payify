using LinkPara.Identity.Application.Features.Users.Commands.VerifyEmail;
using LinkPara.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Common.Interfaces
{
    public interface IUserEmailService
    {
        Task SendEmailVerificationMailAsync(User user);
        Task<bool> VerifyEmailTokenAsync(VerifyEmailCommand command, CancellationToken cancellationToken);
        Task<bool> VerifyEmailChangeTokenAsync(VerifyEmailChangeCommand command, CancellationToken cancellationToken);
        Task SendEmailChangeMailAsync(User user, string newMail);
        Task SendCorporateUserOnboardingMailAsync(User user, Dictionary<string, string> mailParameters);
    }
}
