using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.DeletePricingCommercial;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Commands.UpdatePricingCommercial;

public class UpdateCommercialPricingCommand : IRequest
{
    public Guid Id { get; set; }
    public int MaxDistinctSenderCount { get; set; }
    public int MaxDistinctSenderCountWithAmount { get; set; }
    public decimal MaxDistinctSenderAmount { get; set; }
    public PricingCommercialType PricingCommercialType { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal CommissionRate { get; set; }
    public string CurrencyCode { get; set; }
}

public class UpdateCommercialPricingCommandHandler : IRequestHandler<UpdateCommercialPricingCommand>
{
    private readonly IGenericRepository<PricingCommercial> _commercialPricing;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<UpdateCommercialPricingCommandHandler> _logger;

    public UpdateCommercialPricingCommandHandler(
        IGenericRepository<PricingCommercial> commercialPricing,
        IContextProvider contextProvider,
        ILogger<UpdateCommercialPricingCommandHandler> logger)
    {
        _commercialPricing = commercialPricing;
        _contextProvider = contextProvider;
        _logger = logger;
    }
    public async Task<Unit> Handle(UpdateCommercialPricingCommand request, CancellationToken cancellationToken)
    {
        var sameDatePricings =
            await _commercialPricing
                    .GetAll()
                .Where(s => 
                    s.ActivationDate == request.ActivationDate
                    && s.PricingCommercialStatus != PricingCommercialStatus.Deleted
                    && s.Id != request.Id)
                .ToListAsync(cancellationToken);

        var isDuplicated = CheckDuplication(request.PricingCommercialType, sameDatePricings);
        if (isDuplicated)
        {
            throw new DuplicateRecordException();
        }

        var pricingHistory =
            await _commercialPricing
                .GetAll()
                .Where(s => 
                    s.Id == request.Id 
                    && s.PricingCommercialStatus == PricingCommercialStatus.Waiting)
                .FirstOrDefaultAsync(cancellationToken);
        
        if (pricingHistory != null)
        {
            pricingHistory.MaxDistinctSenderAmount = request.MaxDistinctSenderAmount;
            pricingHistory.MaxDistinctSenderCountWithAmount = request.MaxDistinctSenderCountWithAmount;
            pricingHistory.MaxDistinctSenderCount = request.MaxDistinctSenderCount;
            pricingHistory.PricingCommercialType = request.PricingCommercialType;
            pricingHistory.CommissionRate = request.CommissionRate;
            pricingHistory.CurrencyCode = request.CurrencyCode;
            pricingHistory.ActivationDate = request.ActivationDate;
            pricingHistory.UpdateDate = DateTime.Now;
            pricingHistory.LastModifiedBy = _contextProvider.CurrentContext.UserId;
            await _commercialPricing.UpdateAsync(pricingHistory);
        }
        else
        {
            _logger.LogError($"Record:{request.Id} Status:Waiting not found!");
            throw new NotFoundException(nameof(PricingCommercial), request.Id);
        }
        return Unit.Value;
    }
    private bool CheckDuplication(PricingCommercialType pricingCommercialType, IEnumerable<PricingCommercial> isDuplicatedDate)
    {
        return pricingCommercialType == PricingCommercialType.All
            ? isDuplicatedDate.Any()
            : isDuplicatedDate.Any(s => 
                s.PricingCommercialType == PricingCommercialType.All 
                || s.PricingCommercialType == pricingCommercialType);
    }
}
