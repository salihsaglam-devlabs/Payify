using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class PaymentDetailService : IPaymentDetailService
{
    private readonly ILogger<PaymentDetailService> _logger;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    public PaymentDetailService(VposServiceFactory vposServiceFactory,
        IGenericRepository<Vpos> vposRepository,
        ILogger<PaymentDetailService> logger,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        IGenericRepository<MerchantVpos> merchantVposRepository)
    {
        _vposServiceFactory = vposServiceFactory;
        _vposRepository = vposRepository;
        _logger = logger;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _merchantVposRepository = merchantVposRepository;
    }
    public async Task<PosPaymentDetailResponse> GetPaymentDetailAsync(string orderId)
    {
        var posResponse = new PosPaymentDetailResponse();
        var timeoutTransaction = await _timeoutTransactionRepository.GetAll().Where(b => b.OriginalOrderId == orderId).FirstOrDefaultAsync();
        var vpos = await _vposRepository.GetAll().Include(x => x.AcquireBank).Include(c => c.MerchantVposList).Include(a => a.VposBankApiInfos).ThenInclude(c => c.Key).Where(b => b.Id == timeoutTransaction.VposId).FirstOrDefaultAsync();
        var referenceBankTransaction = await _bankTransactionRepository.GetAll()
                                                    .FirstOrDefaultAsync(s =>
                                                        s.RecordStatus == RecordStatus.Active &&
                                                        s.OrderId == orderId);

        var subMerchant = await _merchantVposRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                      s.TerminalStatus == TerminalStatus.Active &&
                                      s.VposId == vpos.Id &&
                                      s.MerchantId == timeoutTransaction.MerchantId);

        var bankService = _vposServiceFactory.GetVposServices(vpos, timeoutTransaction.MerchantId, vpos.IsInsuranceVpos);
        try
        {
            posResponse = await bankService.PaymentDetail(new PosPaymentDetailRequest
            {
                OrderNumber = orderId,
                LanguageCode = timeoutTransaction.LanguageCode,
                Currency = timeoutTransaction.Currency,
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now,
                SubMerchantCode = vpos.MerchantVposList?.FirstOrDefault()?.SubMerchantCode,
                Password = vpos.MerchantVposList?.FirstOrDefault()?.Password,
                OrderDate = timeoutTransaction.TransactionDate,
                RRN = referenceBankTransaction?.RrnNumber,
                ServiceProviderPspMerchantId = subMerchant.ServiceProviderPspMerchantId
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetPaymentDetailAsyncError : {exception}");
        }

        return posResponse;
    }
}
