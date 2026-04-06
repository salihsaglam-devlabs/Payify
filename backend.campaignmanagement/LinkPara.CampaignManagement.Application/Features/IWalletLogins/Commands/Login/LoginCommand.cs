using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletLogins.Commands.Login;

public class LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Language { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IIWalletLoginService _loginService;

    public LoginCommandHandler(IIWalletLoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _loginService.LoginAsync(request);
    }
}