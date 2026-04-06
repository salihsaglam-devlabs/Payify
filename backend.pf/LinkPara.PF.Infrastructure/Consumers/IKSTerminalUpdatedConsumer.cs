using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class IKSTerminalUpdatedConsumer : IConsumer<IKSTerminalUpdated>
{
    private readonly IGenericRepository<MerchantVpos> _repository;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;
    private readonly ILogger<IKSTerminalUpdatedConsumer> _logger;

    public IKSTerminalUpdatedConsumer(IGenericRepository<MerchantVpos> repository, ILogger<IKSTerminalUpdatedConsumer> logger, IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository)
    {
        _repository = repository;
        _logger = logger;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
    }

    public async Task Consume(ConsumeContext<IKSTerminalUpdated> context)
    {
        try
        {
            if (context.Message.Type == "O")
            {
                var merchantPhysicalPos = await _merchantPhysicalPosRepository
                               .GetAll()
                               .Where(b => b.PosTerminalId == context.Message.OldTerminalId && b.RecordStatus == RecordStatus.Active)
                               .FirstOrDefaultAsync();

                if (merchantPhysicalPos is not null)
                {
                    if (!string.IsNullOrWhiteSpace(context.Message.TerminalId))
                    {
                        merchantPhysicalPos.PosTerminalId = context.Message.TerminalId;
                        merchantPhysicalPos.BkmReferenceNumber = context.Message.ReferenceCode;
                        merchantPhysicalPos.PosMerchantId = context.Message.ServiceProviderPspMerchantId;
                        merchantPhysicalPos.TerminalStatus = TerminalStatus.Active;

                    }

                    if (context.Message.ResponseCode != null && context.Message.ResponseCode != "00" && context.Message.ResponseCode != "200")
                    {
                        merchantPhysicalPos.TerminalStatus = TerminalStatus.Reject;
                        merchantPhysicalPos.RecordStatus = RecordStatus.Passive;
                    }

                    if (context.Message.StatusCode != "0")
                    {
                        merchantPhysicalPos.TerminalStatus = TerminalStatus.Passive;
                        merchantPhysicalPos.RecordStatus = RecordStatus.Passive;
                    }

                    await _merchantPhysicalPosRepository.UpdateAsync(merchantPhysicalPos);
                }
                else
                {
                    var bkmMerchantPhysicalPos = await _merchantPhysicalPosRepository
                                         .GetAll()
                                         .Where(b => b.BkmReferenceNumber == context.Message.ReferenceCode && b.RecordStatus == RecordStatus.Active)
                                         .FirstOrDefaultAsync();

                    if (bkmMerchantPhysicalPos is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(context.Message.TerminalId))
                        {
                            bkmMerchantPhysicalPos.PosTerminalId = context.Message.TerminalId;
                            bkmMerchantPhysicalPos.BkmReferenceNumber = context.Message.ReferenceCode;
                            bkmMerchantPhysicalPos.PosMerchantId = context.Message.ServiceProviderPspMerchantId;
                            bkmMerchantPhysicalPos.TerminalStatus = TerminalStatus.Active;
                        }

                        if (context.Message.ResponseCode != null && context.Message.ResponseCode != "00" && context.Message.ResponseCode != "200")
                        {
                            bkmMerchantPhysicalPos.TerminalStatus = TerminalStatus.Reject;
                            bkmMerchantPhysicalPos.RecordStatus = RecordStatus.Passive;
                        }

                        if (context.Message.StatusCode != "0")
                        {
                            bkmMerchantPhysicalPos.TerminalStatus = TerminalStatus.Passive;
                            bkmMerchantPhysicalPos.RecordStatus = RecordStatus.Passive;
                        }

                        await _merchantPhysicalPosRepository.UpdateAsync(bkmMerchantPhysicalPos);
                    }
                }
            }
            else
            {
                var merchantVpos = await _repository
                              .GetAll()
                              .Where(b => b.SubMerchantCode == context.Message.OldTerminalId && b.RecordStatus == RecordStatus.Active)
                              .FirstOrDefaultAsync();

                if (merchantVpos is not null)
                {
                    if (!string.IsNullOrWhiteSpace(context.Message.TerminalId))
                    {
                        merchantVpos.SubMerchantCode = context.Message.TerminalId;
                        merchantVpos.BkmReferenceNumber = context.Message.ReferenceCode;
                        merchantVpos.ServiceProviderPspMerchantId = context.Message.ServiceProviderPspMerchantId;
                        merchantVpos.TerminalStatus = TerminalStatus.Active;

                    }

                    if (context.Message.ResponseCode != null && context.Message.ResponseCode != "00" && context.Message.ResponseCode != "200")
                    {
                        merchantVpos.TerminalStatus = TerminalStatus.Reject;
                        merchantVpos.RecordStatus = RecordStatus.Passive;
                    }

                    if (context.Message.StatusCode != "0")
                    {
                        merchantVpos.TerminalStatus = TerminalStatus.Passive;
                        merchantVpos.RecordStatus = RecordStatus.Passive;
                    }

                    await _repository.UpdateAsync(merchantVpos);
                }
                else
                {
                    var bkmMerchantVpos = await _repository
                                         .GetAll()
                                         .Where(b => b.BkmReferenceNumber == context.Message.ReferenceCode && b.RecordStatus == RecordStatus.Active)
                                         .FirstOrDefaultAsync();

                    if (bkmMerchantVpos is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(context.Message.TerminalId))
                        {
                            bkmMerchantVpos.SubMerchantCode = context.Message.TerminalId;
                            bkmMerchantVpos.BkmReferenceNumber = context.Message.ReferenceCode;
                            bkmMerchantVpos.ServiceProviderPspMerchantId = context.Message.ServiceProviderPspMerchantId;
                            bkmMerchantVpos.TerminalStatus = TerminalStatus.Active;
                        }

                        if (context.Message.ResponseCode != null && context.Message.ResponseCode != "00" && context.Message.ResponseCode != "200")
                        {
                            bkmMerchantVpos.TerminalStatus = TerminalStatus.Reject;
                            bkmMerchantVpos.RecordStatus = RecordStatus.Passive;
                        }

                        if (context.Message.StatusCode != "0")
                        {
                            bkmMerchantVpos.TerminalStatus = TerminalStatus.Passive;
                            bkmMerchantVpos.RecordStatus = RecordStatus.Passive;
                        }

                        await _repository.UpdateAsync(bkmMerchantVpos);
                    }
                }
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IKSTerminalUpdatedConsumer failed. ReferenceNumber: {ReferenceNumber}, SubMerchantCode: {SubMerchantCode}",
               context.Message.ReferenceCode, context.Message.OldTerminalId);
        }
    }
}
