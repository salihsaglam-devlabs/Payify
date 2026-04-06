using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Commands.UpdateMerchantPreApplication;

public class UpdateMerchantPreApplicationCommand : IRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string ResponsiblePerson { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public PosProductType ProductTypes { get; set; }
    public MonthlyTurnover MonthlyTurnover { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
    public string Website { get; set; }
    public List<MerchantPreApplicationHistoryDto> ApplicationHistories { get; set; }
}

public class UpdatePendingPosApplicationCommandHandler : IRequestHandler<UpdateMerchantPreApplicationCommand>
{
    private readonly IMerchantPreApplicationService _merchantPreApplicationService;

    public UpdatePendingPosApplicationCommandHandler(IMerchantPreApplicationService merchantPreApplicationService)
    {
        _merchantPreApplicationService = merchantPreApplicationService;
    }

    public async Task<Unit> Handle(UpdateMerchantPreApplicationCommand request, CancellationToken cancellationToken)
    {
        await _merchantPreApplicationService.UpdatePosApplicationAsync(request);
        
        return Unit.Value;
    }
}