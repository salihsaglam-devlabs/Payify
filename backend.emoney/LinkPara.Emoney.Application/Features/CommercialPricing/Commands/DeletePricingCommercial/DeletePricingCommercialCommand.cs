using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Commands.DeletePricingCommercial;

public class DeletePricingCommercialCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePricingCommercialCommandHandler : IRequestHandler<DeletePricingCommercialCommand>
{
    private readonly IGenericRepository<PricingCommercial> _commercialPricing;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<DeletePricingCommercialCommandHandler> _logger;

    public DeletePricingCommercialCommandHandler(
        IGenericRepository<PricingCommercial> commercialPricing,
        IContextProvider contextProvider,
        ILogger<DeletePricingCommercialCommandHandler> logger)
    {
        _commercialPricing = commercialPricing;
        _contextProvider = contextProvider;
        _logger = logger;
    }
    public async Task<Unit> Handle(DeletePricingCommercialCommand request, CancellationToken cancellationToken)
    {
        var commercialPricing = await _commercialPricing.GetByIdAsync(request.Id);

        if (commercialPricing is null)
        {
            _logger.LogError($"Record:{request.Id} not found!");
            throw new NotFoundException(nameof(PricingCommercial), request.Id);
        }
        if (commercialPricing.PricingCommercialStatus is not 
            (PricingCommercialStatus.Waiting or PricingCommercialStatus.InUse))
        {
            throw new CommercialPricingStatusInvalidException();
        }
        
        commercialPricing.PricingCommercialStatus = PricingCommercialStatus.Deleted;
        commercialPricing.LastModifiedBy = _contextProvider.CurrentContext.UserId;
        commercialPricing.RecordStatus = RecordStatus.Passive;
        await _commercialPricing.UpdateAsync(commercialPricing);
        
        return Unit.Value;
    }
}
