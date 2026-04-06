using System.Transactions;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class DeletePaymentLinkConsumer : IConsumer<DeletePaymentLink>
{
    
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly IGenericRepository<HostedPayment> _hostedPaymentRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IHostedPaymentService _hostedPaymentService;
    private readonly IGenericRepository<OnUsPayment> _onUsPaymentRepository;
    private readonly IOnUsPaymentService _onUsPaymentService;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public DeletePaymentLinkConsumer(
        IGenericRepository<Link> linkRepository, 
        IGenericRepository<HostedPayment> hostedPaymentRepository,
        IApplicationUserService applicationUserService,
        IHostedPaymentService hostedPaymentService,
        IGenericRepository<OnUsPayment> onUsPaymentRepository,
        IOnUsPaymentService onUsPaymentService,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        IResponseCodeService errorCodeService,
        IServiceScopeFactory scopeFactory)
    {
        _linkRepository = linkRepository;
        _hostedPaymentRepository = hostedPaymentRepository;
        _applicationUserService = applicationUserService;
        _hostedPaymentService = hostedPaymentService;
        _onUsPaymentRepository = onUsPaymentRepository;
        _onUsPaymentService = onUsPaymentService;
        _merchantTransactionRepository = merchantTransactionRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _errorCodeService = errorCodeService;
        _scopeFactory = scopeFactory;
    }
    
     public async Task Consume(ConsumeContext<DeletePaymentLink> context)
     {
         var links = await _linkRepository
             .GetAll()
             .Where(l => l.ExpiryDate < DateTime.Now && l.RecordStatus == RecordStatus.Active)
             .ToListAsync();
         
         if (links.Any())
         {
             links.ForEach(l =>
             {
                 l.UpdateDate = DateTime.Now;
                 l.RecordStatus = RecordStatus.Passive;
                 l.LinkStatus = ChannelStatus.Expired;
                 l.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
             });
             
             await _linkRepository.UpdateRangeAsync(links);
         }
         
         var hostedPayments = await _hostedPaymentRepository
             .GetAll()
             .Where(l => l.ExpiryDate < DateTime.Now && l.RecordStatus == RecordStatus.Active)
             .ToListAsync();
         
         if (hostedPayments.Any())
         {
             var trackingIds = new List<string>();
             hostedPayments.ForEach(hpp =>
             {
                 hpp.UpdateDate = DateTime.Now;
                 hpp.RecordStatus = RecordStatus.Passive;
                 hpp.HppStatus = ChannelStatus.Expired;
                 hpp.HppPaymentStatus = ChannelPaymentStatus.Expired;
                 hpp.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                 trackingIds.Add(hpp.TrackingId);
             });
             
             await _hostedPaymentRepository.UpdateRangeAsync(hostedPayments);
             
             foreach (var trackingId in trackingIds)
             {
                 await _hostedPaymentService.TriggerHppWebhookAsync(trackingId);
             }
         }

         var onUsPayments = await _onUsPaymentRepository
             .GetAll()
             .Where(s => s.Status == ChannelStatus.Active && s.ExpiryDate < DateTime.Now)
             .ToListAsync();

         if (onUsPayments.Any())
         {
             var merchantTransactionIds = onUsPayments.Select(a => a.MerchantTransactionId).ToList();
             
             var merchantTransactions = await _merchantTransactionRepository
                 .GetAll()
                 .Where(s => merchantTransactionIds.Contains(s.Id))
                 .ToListAsync();

             var bankTransactions = await _bankTransactionRepository
                 .GetAll()
                 .Where(s => merchantTransactionIds.Contains(s.MerchantTransactionId))
                 .ToListAsync();

             foreach (var onUs in onUsPayments)
             {
                 var merchantTransaction = merchantTransactions.FirstOrDefault(s => s.Id == onUs.MerchantTransactionId);
                 var bankTransaction =
                     bankTransactions.FirstOrDefault(s => s.MerchantTransactionId == onUs.MerchantTransactionId);
                 
                 using var scope = _scopeFactory.CreateScope();
                 var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
                 var strategy = dbContext.Database.CreateExecutionStrategy();
                 await strategy.ExecuteAsync(async () =>
                 {
                     using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    
                     onUs.UpdateDate = DateTime.Now;
                     onUs.Status = ChannelStatus.Expired;
                     onUs.PaymentStatus = ChannelPaymentStatus.Expired;
                     onUs.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                     dbContext.OnUsPayment.Update(onUs);
                     
                     var merchantResponse = await _errorCodeService.GetApiResponseCode(ApiErrorCode.OnUsExpired, merchantTransaction?.LanguageCode ?? "tr");

                     if (merchantTransaction is not null)
                     {
                         merchantTransaction.ResponseCode = merchantResponse.ResponseCode;
                         merchantTransaction.ResponseDescription = merchantResponse.DisplayMessage;
                         merchantTransaction.TransactionEndDate = DateTime.Now;
                         merchantTransaction.TransactionStatus = TransactionStatus.Fail;
                         dbContext.MerchantTransaction.Update(merchantTransaction);
                     }

                     if (bankTransaction is not null)
                     {
                         bankTransaction.BankResponseCode = merchantResponse.ResponseCode;
                         bankTransaction.BankResponseDescription = merchantResponse.DisplayMessage;
                         bankTransaction.TransactionEndDate = DateTime.Now;
                         bankTransaction.TransactionStatus = TransactionStatus.Fail;
                         bankTransaction.BankTransactionDate = DateTime.Now;
                         dbContext.BankTransaction.Update(bankTransaction);
                     }

                     await dbContext.SaveChangesAsync();
                     
                     await _onUsPaymentService.TriggerOnUsWebhookAsync(onUs.Id);
                     
                     transactionScope.Complete();
                 });
             }
         }
     }
    
}