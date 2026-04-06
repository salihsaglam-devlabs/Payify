using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class CheckMerchantVposStatusConsumer : IConsumer<MerchantVposStatus>
{
    private readonly ILogger<CheckMerchantVposStatusConsumer> _logger;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IVaultClient _vaultClient;
    private readonly IIksPfService _iksPfService;
    
    public CheckMerchantVposStatusConsumer(
        ILogger<CheckMerchantVposStatusConsumer> logger,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<MerchantVpos> merchantVposRepository,
        IVaultClient vaultClient,
        IIksPfService iksPfService)
    {
        _logger = logger;
        _merchantRepository = merchantRepository;
        _merchantVposRepository = merchantVposRepository;
        _vaultClient = vaultClient;
        _iksPfService = iksPfService;
    }
    
    public async Task Consume(ConsumeContext<MerchantVposStatus> context)
    {
        var isIksEnabled =
            await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "IksEnabled");

        if (!isIksEnabled)
        {
            return;
        }

        var merchantList = await _merchantVposRepository.GetAll()
            .Include(v => v.Vpos)
            .Where(v => v.TerminalStatus == TerminalStatus.PendingRequest
                            && v.RecordStatus == RecordStatus.Active
                            && v.Vpos.VposStatus == VposStatus.Active)
            .Select(v => v.MerchantId)
            .Distinct()
            .ToListAsync();

        var merchants = await _merchantRepository.GetAll()
            .Include(b => b.MerchantVposList).ThenInclude(c => c.Vpos)
            .Include(b => b.Customer)
            .ThenInclude(b => b.AuthorizedPerson)
            .Where(m => merchantList.Contains(m.Id) 
                        && m.MerchantStatus == MerchantStatus.Active
                        && m.RecordStatus == RecordStatus.Active)
            .ToListAsync();
        
        foreach (var merchant in merchants)
        {
            try
            {
                await _iksPfService.IKSCreateTerminalAsync(merchant);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching VPOS status from IKS for Merchant {merchant.Id}: {ex.Message}");
            }
        }
    }
}