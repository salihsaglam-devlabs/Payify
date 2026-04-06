using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LinkPara.IKS.Infrastructure.Consumers.CronJobs;

public class CreateTerminalResponseCheckConsumer : IConsumer<CreateTerminalResponseCheck>
{
    private readonly ILogger<CreateTerminalResponseCheckConsumer> _logger;
    private readonly IGenericRepository<IksTerminal> _iksTerminalRepository;
    private readonly IGenericRepository<IksTerminalHistory> _iksTerminalHistoryRepository;
    private readonly IGenericRepository<IKSTransaction> _iksTransactionRepository;
    private readonly IIKSService _iksService;
    private readonly IBus _bus;

    public CreateTerminalResponseCheckConsumer(ILogger<CreateTerminalResponseCheckConsumer> logger,
        IGenericRepository<IksTerminal> iksTerminalRepository,
        IGenericRepository<IKSTransaction> iksTransactionRepository,
        IIKSService iksService,
        IBus bus,
        IGenericRepository<IksTerminalHistory> iksTerminalHistoryRepository)
    {
        _logger = logger;
        _iksTerminalRepository = iksTerminalRepository;
        _iksTransactionRepository = iksTransactionRepository;
        _iksService = iksService;
        _bus = bus;
        _iksTerminalHistoryRepository = iksTerminalHistoryRepository;
    }

    public async Task Consume(ConsumeContext<CreateTerminalResponseCheck> context)
    {
        try
        {
            var lastExecution = await _iksTransactionRepository.GetAll()
                .Where(s => s.Operation == "CheckTerminalStatusCron" && s.IsSuccess == true)
                .OrderByDescending(s => s.CreateDate).FirstOrDefaultAsync();

            DateTime startDate;
            var endDate = DateTime.Now;
            if (lastExecution is null)
            {
                startDate = endDate.AddDays(-30);
            }
            else
            {
                if (lastExecution.RequestDetails != null &&
                    lastExecution.RequestDetails.TryGetValue("EndDate", out var endDateStr) &&
                    endDateStr is DateTime lastEndDate)
                {
                    startDate = lastEndDate;
                }
                else
                {
                    startDate = lastExecution.CreateDate.AddDays(-1);
                }
            }
            
            var offset = 0;
            const int limit = 1000;
            var responseTerminals = new List<IKSTerminal>();
            
            while (startDate < endDate)
            {
                var tempEndDate = startDate.AddDays(30);
                if (tempEndDate > endDate)
                    tempEndDate = endDate;
                
                var endOfPage = false;
                
                while (!endOfPage)
                {
                    var checkUpdatedTerminals = await _iksService.GetTerminalStatusQueryAsync(new GetTerminalStatusRequest
                    {
                        StartDate = startDate,
                        EndDate = tempEndDate,
                        Offset = offset,
                        Limit = limit
                    });

                    if (checkUpdatedTerminals.IsSuccess)
                    {
                        if (checkUpdatedTerminals.Data?.terminals.Count > 0)
                        {
                            responseTerminals = responseTerminals
                                .Concat(checkUpdatedTerminals.Data.terminals)
                                .DistinctBy(e => e.referenceCode)
                                .ToList();
                        }

                        if (checkUpdatedTerminals.Data?.totalCount < limit + offset)
                        {
                            endOfPage = true;
                        }
                        else
                        {
                            offset += limit;
                        }
                    }
                    else
                    {
                        endOfPage = true;
                        _logger.LogError($"CreateTerminalResponseCheckConsumer IKS service error: {checkUpdatedTerminals.Error.moreInformation}");
                    }
                }

                startDate = tempEndDate;
            }

            if (responseTerminals.Count <= 0)
            {
                return;
            }

            var responseReferenceCodes = responseTerminals.Select(s => s.referenceCode).ToList();
            var dbTerminals = await _iksTerminalRepository.GetAll()
                .Where(s => responseReferenceCodes.Contains(s.ReferenceCode)).ToListAsync();
            
            var responseTerminalDict = responseTerminals.ToDictionary(r => r.referenceCode, r => r);
            var updatedList = new List<IksTerminal>();
            var terminalHistories = new List<IksTerminalHistory>();

            foreach (var dbTerminal in dbTerminals)
            {
                if (!responseTerminalDict.TryGetValue(dbTerminal.ReferenceCode, out var responseTerminal))
                    continue;

                var oldTerminalId = dbTerminal.TerminalId;
                var oldResponseCode = dbTerminal.ResponseCode;
                var oldStatusCode = dbTerminal.StatusCode;
                var updated = _iksService.UpdateIksTerminalFields(dbTerminal, responseTerminal);

                if (updated.TerminalId != oldTerminalId || updated.StatusCode != oldStatusCode)
                {
                    try
                    {
                        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.IKSTerminalIdUpdated"));
                        await endpoint.Send(new IKSTerminalUpdated
                        {
                            GlobalMerchantId = responseTerminal.globalMerchantId,
                            PspMerchantId = responseTerminal.pspMerchantId,
                            TerminalId = responseTerminal.terminalId,
                            OldTerminalId = oldTerminalId,
                            ReferenceCode = responseTerminal.referenceCode,
                            StatusCode = responseTerminal.statusCode,
                            ResponseCode = responseTerminal.responseCode,
                            ResponseCodeExplanation = responseTerminal.responseCodeExplanation,
                            ServiceProviderPspMerchantId = responseTerminal.serviceProviderPspMerchantId,
                            Type = responseTerminal.type,   
                        }, tokenSource.Token);
                        updatedList.Add(updated);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"CreateTerminalResponseCheckConsumer - MerchantId({updated.MerchantId}) ReferenceCode({updated.ReferenceCode}) Error : {exception}");
                    }
                  
                    if (updated.TerminalId != oldTerminalId)
                    {
                        terminalHistories.Add(new IksTerminalHistory
                        {
                            MerchantId = updated.MerchantId,
                            VposId = updated.VposId,
                            PhysicalPosId = updated.PhysicalPosId,
                            OldData = oldTerminalId ?? string.Empty,
                            NewData = updated.TerminalId ?? string.Empty,
                            ChangedField = "TerminalId",
                            ReferenceCode = responseTerminal.referenceCode,
                            ResponseCode = responseTerminal.responseCode,
                            ResponseCodeExplanation = responseTerminal.responseCodeExplanation,
                            CreatedBy = Guid.Empty.ToString(),
                            QueryDate = DateTime.Now,
                            TerminalRecordType = TerminalRecordType.BkmTerminalUpdate
                        });
                    }

                    if (updated.StatusCode != oldStatusCode)
                    {
                        terminalHistories.Add(new IksTerminalHistory
                        {
                            MerchantId = updated.MerchantId,
                            VposId = updated.VposId,
                            PhysicalPosId = updated.PhysicalPosId,
                            OldData = oldStatusCode?.ToString() ?? string.Empty,
                            NewData = updated.StatusCode?.ToString() ?? string.Empty,
                            ChangedField = "StatusCode",
                            ReferenceCode = responseTerminal.referenceCode,
                            ResponseCode = responseTerminal.responseCode,
                            ResponseCodeExplanation = responseTerminal.responseCodeExplanation,
                            CreatedBy = Guid.Empty.ToString(),
                            QueryDate = DateTime.Now,
                            TerminalRecordType = TerminalRecordType.BkmTerminalUpdate
                        });
                    }

                    if (updated.ResponseCode != oldResponseCode)
                    {
                        terminalHistories.Add(new IksTerminalHistory
                        {
                            MerchantId = updated.MerchantId,
                            VposId = updated.VposId,
                            PhysicalPosId = updated.PhysicalPosId,
                            OldData = oldResponseCode?.ToString() ?? string.Empty,
                            NewData = updated.ResponseCode?.ToString() ?? string.Empty,
                            ChangedField = "ResponseCode",
                            ReferenceCode = responseTerminal.referenceCode,
                            ResponseCode = responseTerminal.responseCode,
                            ResponseCodeExplanation = responseTerminal.responseCodeExplanation,
                            CreatedBy = Guid.Empty.ToString(),
                            QueryDate = DateTime.Now,
                            TerminalRecordType = TerminalRecordType.BkmTerminalUpdate
                        });
                    }
                }
            }

            if (terminalHistories.Any())
            {
                await _iksTerminalHistoryRepository.AddRangeAsync(terminalHistories);
            }

            await _iksTerminalRepository.UpdateRangeAsync(updatedList);
        }
        catch (Exception exception)
        {
            _logger.LogError($"CreateTerminalResponseCheckConsumer: {exception}");
        }
    }
}