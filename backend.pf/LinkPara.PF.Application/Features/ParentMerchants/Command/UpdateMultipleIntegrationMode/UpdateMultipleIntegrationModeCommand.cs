using AutoMapper;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.MainSubMerchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultipleIntegrationMode;

public class UpdateMultipleIntegrationModeCommand : IRequest, IMapFrom<Merchant>
{
    public List<UpdateMultipleIntegrationModeModel> MultipleIntegrationModeModel { get; set; }
}
public class UpdateMultipleIntegrationModeCommandHandler : IRequestHandler<UpdateMultipleIntegrationModeCommand>
{
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILogger<UpdateMultipleIntegrationModeCommandHandler> _logger;

    public UpdateMultipleIntegrationModeCommandHandler(
        IGenericRepository<Merchant> merchantRepository,
        ILogger<UpdateMultipleIntegrationModeCommandHandler> logger)
    {
        _merchantRepository = merchantRepository;
        _logger = logger;
    }
    public async Task<Unit> Handle(UpdateMultipleIntegrationModeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in request.MultipleIntegrationModeModel)
            {
                var merchant = await _merchantRepository.GetByIdAsync(item.MainSubMerchantId);

                if (merchant is null)
                {
                    throw new NotFoundException(nameof(Merchant), item.MainSubMerchantId);
                }

                merchant.IntegrationMode = item.IntegrationMode;

                await _merchantRepository.UpdateAsync(merchant);
            }

            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateMultipleIntegrationModeCommandError : {exception}");
            throw;
        }
    }
}