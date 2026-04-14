using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

public class AddAdditionalLimitRestrictionCommand : IRequest<PaycoreResponse>
{
    public string CardNo { get; set; }
    public AdditionalLimit[] AdditionalLimits { get; set; }
    public LimitRestriction[]? LimitRestrictions { get; set; }
}

public class AddAdditionalLimitRestrictionCommandHandler : IRequestHandler<AddAdditionalLimitRestrictionCommand, PaycoreResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public AddAdditionalLimitRestrictionCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<PaycoreResponse> Handle(AddAdditionalLimitRestrictionCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreService.AddAdditionalLimitRestrictionAsync(request);
    }
}
