using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Commands.UpdateUser;
using LinkPara.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LinkPara.Identity.Application.Features.Users.Commands.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<bool>
    {
        public string Token { get; set; }
        public string Username { get; set; }
    }
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
    {
        private readonly IUserEmailService _userEmailService;

        public VerifyEmailCommandHandler(IUserEmailService userEmailService)
        {
            _userEmailService = userEmailService;
        }
        public async Task<bool> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            var result=await _userEmailService.VerifyEmailTokenAsync(command,cancellationToken);
            return result;
        }
    }
}
