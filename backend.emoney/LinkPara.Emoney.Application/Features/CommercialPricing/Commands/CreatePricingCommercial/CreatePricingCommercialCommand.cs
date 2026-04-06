using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Commands.CreatePricingCommercial;

public class CreatePricingCommercialCommand : IRequest
{
    public int MaxDistinctSenderCount { get; set; }
    public int MaxDistinctSenderCountWithAmount { get; set; }
    public decimal MaxDistinctSenderAmount { get; set; }
    public PricingCommercialType PricingCommercialType { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal CommissionRate { get; set; }
    public string CurrencyCode { get; set; }
}

public class CreateCommercialPricingCommandHandler : IRequestHandler<CreatePricingCommercialCommand>
{ 
    private readonly IGenericRepository<PricingCommercial> _commercialPricing;
    private readonly IContextProvider _contextProvider;

    public CreateCommercialPricingCommandHandler(IGenericRepository<PricingCommercial> commercialPricing,  IContextProvider contextProvider)
    {
        _commercialPricing = commercialPricing;
        _contextProvider = contextProvider;
    }
    public async Task<Unit> Handle(CreatePricingCommercialCommand request, CancellationToken cancellationToken)
    {
        var sameDatePricings =
            await _commercialPricing
                .GetAll()
                .Where(s => 
                    s.ActivationDate == request.ActivationDate
                    && s.PricingCommercialStatus != PricingCommercialStatus.Deleted)
                .ToListAsync(cancellationToken);

        var isDuplicated = CheckDuplication(request.PricingCommercialType, sameDatePricings);

        if (isDuplicated)
        {
            throw new DuplicateRecordException();
        }

        var newCommercialPriceHistory = new PricingCommercial
        {
            Id = Guid.NewGuid(),
            MaxDistinctSenderAmount = request.MaxDistinctSenderAmount,
            MaxDistinctSenderCountWithAmount = request.MaxDistinctSenderCountWithAmount,
            MaxDistinctSenderCount = request.MaxDistinctSenderCount,
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            LastModifiedBy = _contextProvider.CurrentContext.UserId,
            RecordStatus = RecordStatus.Active,
            PricingCommercialType = request.PricingCommercialType,
            ActivationDate = request.ActivationDate,
            CommissionRate = request.CommissionRate,
            CurrencyCode = request.CurrencyCode,
            PricingCommercialStatus = PricingCommercialStatus.Waiting
        };
        await _commercialPricing.AddAsync(newCommercialPriceHistory);
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