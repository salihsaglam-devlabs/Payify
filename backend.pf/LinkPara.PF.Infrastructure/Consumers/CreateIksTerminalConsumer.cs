using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.IKS;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class CreateIksTerminalConsumer : IConsumer<CreateIksTerminal>
{
    private readonly IIksPfService _iksPfService;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILogger<CreateIksTerminalConsumer> _logger;

    public CreateIksTerminalConsumer(IIksPfService iksPfService, IGenericRepository<Merchant> merchantRepository, ILogger<CreateIksTerminalConsumer> logger)
    {
        _iksPfService = iksPfService;
        _merchantRepository = merchantRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateIksTerminal> context)
    {
        var posType = context.Message.PosType;
        _logger.LogError($"Message.PosType : {posType}");

        IQueryable<Merchant> query = _merchantRepository.GetAll();

        switch (posType)
        {
            case PosType.Physical:
                query = query
                    .Include(m => m.MerchantPhysicalDevices)
                        .ThenInclude(d => d.MerchantPhysicalPosList)
                    .Include(m => m.MerchantPhysicalDevices)
                        .ThenInclude(d => d.DeviceInventory);
                break;

            case PosType.Virtual:
                query = query
                    .Include(m => m.MerchantVposList)
                        .ThenInclude(v => v.Vpos);
                break;

            default:
                _logger.LogError(
                    $"CreateIksTerminalConsumer Invalid PosType. MerchantId: {context.Message.MerchantId}");
                return;
        }

        var merchant = await query
            .FirstOrDefaultAsync(m => m.Id == context.Message.MerchantId);

        if (merchant is null)
        {
            _logger.LogError(
                $"CreateIksTerminalConsumer Merchant not found. MerchantId: {context.Message.MerchantId}");
            return;
        }

        if (posType == PosType.Physical)
        {
            _logger.LogError($"Message.PosType : if");
            await _iksPfService.IKSCreatePhysicalTerminalAsync(merchant);
        }
        else 
        {
            _logger.LogError($"Message.PosType : else");
            await _iksPfService.IKSCreateTerminalAsync(merchant);
        }
    }
}
