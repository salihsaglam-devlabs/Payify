using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.IKS;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class CreateIksMerchantConsumer : IConsumer<CreateIksMerchant>
{
    private readonly IIksPfService _iksPfService;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILogger<CreateIksMerchantConsumer> _logger;

    public CreateIksMerchantConsumer(IIksPfService iksPfService, IGenericRepository<Merchant> merchantRepository, 
        ILogger<CreateIksMerchantConsumer> logger)
    {
        _iksPfService = iksPfService;
        _merchantRepository = merchantRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateIksMerchant> context)
    {
        var merchant = await _merchantRepository.GetAll()
            .Include(b => b.MerchantVposList)
                .ThenInclude(c => c.Vpos)
            .Include(b => b.Customer)
                .ThenInclude(b => b.AuthorizedPerson)
            .FirstOrDefaultAsync(b => b.Id == context.Message.MerchantId);

        if (merchant is null)
        {
            _logger.LogError($"CreateIksMerchantConsumer Merchant not found. MerchantId: {context.Message.MerchantId}");
            return;
        }
        
        await _iksPfService.IKSSaveMerchantAsync(merchant);

        if (!string.IsNullOrEmpty(merchant.GlobalMerchantId))
            merchant.MerchantStatus = MerchantStatus.Active;
        
        await _merchantRepository.UpdateAsync(merchant);
    }
}