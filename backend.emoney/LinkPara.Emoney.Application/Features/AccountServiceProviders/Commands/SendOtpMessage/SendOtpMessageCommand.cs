using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendOtpMessage;
public class SendOtpMessageCommand : IRequest<SendOtpMessageResultDto>
{
    public Guid AccountId { get; set; }
    public string Username { get; set; }
    public string SmsContent { get; set; }
}

public class SendOtpMessageCommandHandler : IRequestHandler<SendOtpMessageCommand, SendOtpMessageResultDto>
{
    private readonly IOpenBankingService _openBankingService;

    public SendOtpMessageCommandHandler(IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task<SendOtpMessageResultDto> Handle(SendOtpMessageCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingService.SendOtpMessageAsync(request);
    }
}
