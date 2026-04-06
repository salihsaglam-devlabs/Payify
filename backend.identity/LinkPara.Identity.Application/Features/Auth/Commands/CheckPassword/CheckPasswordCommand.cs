using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Features.OAuth;
using LinkPara.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Auth.Commands.CheckPassword
{
    public class CheckPasswordCommand : IRequest<CheckPasswordDto>
    {
        public string Password { get; set; }
    }
    public class CheckPasswordCommandHandler : IRequestHandler<CheckPasswordCommand, CheckPasswordDto>
    {
        private readonly SignInManager<User> _signInManager;
        private readonly IContextProvider _contextProvider;

        public CheckPasswordCommandHandler(SignInManager<User> signInManager, IContextProvider contextProvider)
        {
            _signInManager = signInManager;
            _contextProvider = contextProvider;
        }

        public async Task<CheckPasswordDto> Handle(CheckPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = _contextProvider.CurrentContext.UserId is not null
                ? await _signInManager.UserManager.FindByIdAsync(_contextProvider.CurrentContext.UserId)
                : null;

            if (user is null)
            {
                return new CheckPasswordDto { IsSuccess = false };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            return new CheckPasswordDto { IsSuccess = result.Succeeded };
        }
    }
}