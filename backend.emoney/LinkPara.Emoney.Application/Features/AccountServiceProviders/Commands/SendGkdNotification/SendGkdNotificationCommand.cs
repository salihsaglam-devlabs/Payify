using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendGkdNotification;
public class SendGkdNotificationCommand : IRequest<SendNotificationResultDto>
{
    public byte CustomerType { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }
    public string DecoupledIdType { get; set; }
    public string DecoupledIdValue { get; set; }
    public string MessageContentTR { get; set; }
    public string MessageContentEN { get; set; }
    public string DeepLink { get; set; }
    public Guid AccountId { get; set; }
}

public class SendGkdNotificationCommandHandler : IRequestHandler<SendGkdNotificationCommand, SendNotificationResultDto>
{
    private readonly IOpenBankingService _openBankingService;

    public SendGkdNotificationCommandHandler(
        IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task<SendNotificationResultDto> Handle(SendGkdNotificationCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingService.SendGkdNotificationAsync(request);
    }
}
