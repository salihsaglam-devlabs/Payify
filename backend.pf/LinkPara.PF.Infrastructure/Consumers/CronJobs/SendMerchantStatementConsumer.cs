using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Models.MerchantStatement;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs
{
    public class SendMerchantStatementConsumer : IConsumer<SendMerchantStatement>
    {
        private readonly ILogger<SendMerchantStatementConsumer> _logger;
        private readonly IBus _bus;
        private readonly IGenericRepository<MerchantStatement> _merchantStatementRepository;
        private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
        private readonly IGenericRepository<Merchant> _merchantRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IParameterService _parameterService;

        public SendMerchantStatementConsumer(
            ILogger<SendMerchantStatementConsumer> logger,
            IGenericRepository<PostingTransaction> postingTransactionRepository,
            IGenericRepository<MerchantStatement> merchantStatementRepository,
            IGenericRepository<Merchant> merchantRepository,
            IApplicationUserService applicationUserService,
            IParameterService parameterService,
            IBus bus)
        {
            _logger = logger;
            _merchantStatementRepository = merchantStatementRepository;
            _postingTransactionRepository = postingTransactionRepository;
            _merchantRepository = merchantRepository;
            _applicationUserService = applicationUserService;
            _parameterService = parameterService;
            _bus = bus;
        }
        
        public async Task Consume(ConsumeContext<SendMerchantStatement> context)
        {
            try
            {
                var statementType = MerchantStatementType.Excel;
                try
                {
                    var statementTypeParameter = await _parameterService.GetParameterAsync("PFParameters", "MerchantStatementType");
                    if (statementTypeParameter is not null)
                    {
                        statementType = statementTypeParameter.ParameterValue switch
                        {
                            "Excel" => MerchantStatementType.Excel,
                            "PDF" => MerchantStatementType.PDF,
                            "Both" => MerchantStatementType.Both,
                            _ => MerchantStatementType.Excel
                        };
                    }
                }
                catch (Exception exception)
                {
                    statementType = MerchantStatementType.Excel;
                }
                
                var startDate = DateTime.Now.Month != 1 ? 
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1) : 
                    new DateTime(DateTime.Now.Year - 1, 12, 1);
                
                var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);

                var merchantIds = await _postingTransactionRepository
                    .GetAll()
                    .Where(
                        s => s.TransactionDate >= startDate && 
                             s.TransactionDate <= endDate &&
                             s.BatchStatus == BatchStatus.Completed)
                    .Select(s => s.MerchantId)
                    .Distinct()
                    .ToListAsync();

                foreach (var merchantId in merchantIds)
                {
                    try
                    {
                        if (await _merchantStatementRepository
                                .GetAll()
                                .Where(s => 
                                    s.MerchantId == merchantId &&
                                    s.StatementStartDate == startDate &&
                                    s.StatementEndDate == endDate &&
                                    s.StatementStatus != MerchantStatementStatus.Failed
                                )
                                .AnyAsync())
                        {
                            continue;
                        }
                        
                        var merchant = await _merchantRepository
                            .GetAll()
                            .Where(b => b.Id == merchantId)
                            .Include(b => b.Customer)
                            .ThenInclude(b => b.AuthorizedPerson)
                            .FirstOrDefaultAsync();

                        if (merchant is null)
                        {
                            _logger.LogError($"CreateMerchantStatementConsumerError: Merchant not found.. MerchantId : {merchantId}");
                            return;
                        }

                        var merchantStatement = new MerchantStatement
                        {
                            MerchantId = merchant.Id,
                            MerchantName = merchant.Customer.CommercialTitle,
                            MailAddress = merchant.Customer.AuthorizedPerson.Email,
                            StatementStartDate = startDate,
                            StatementEndDate = endDate,
                            StatementMonth = startDate.Month,
                            StatementYear = startDate.Year,
                            ReceiptNumber = string.Empty,
                            StatementStatus = MerchantStatementStatus.Pending,
                            StatementType = statementType,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                        };
                        
                        await _merchantStatementRepository.AddAsync(merchantStatement);
                        
                        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.CreateMerchantStatement"));
                        await endpoint.Send(new CreateMerchantStatement { MerchantStatementId = merchantStatement.Id}, tokenSource.Token);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"SendMerchantStatementConsumer - MerchantId({merchantId}) Error : {exception}");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"SendMerchantStatementConsumer: {exception}");
            }
        }
    }
}
