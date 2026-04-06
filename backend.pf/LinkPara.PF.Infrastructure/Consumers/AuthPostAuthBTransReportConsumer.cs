using LinkPara.Cache;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.Location.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.BTrans;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionSource = LinkPara.SharedModels.Banking.Enums.TransactionSource;

namespace LinkPara.PF.Infrastructure.Consumers;

public class AuthPostAuthBTransReportConsumer : IConsumer<AuthPostAuthBTransReport>
{
    private readonly ILogger<AuthPostAuthBTransReportConsumer> _logger;
    private readonly ISourceBankAccountService _sourceBankAccountService;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILocationService _locationService;
    private readonly IBus _bus;
    private readonly ICacheService _cacheService;
    private readonly PfDbContext _dbContext;
    private readonly IGenericRepository<PostingAdditionalTransaction> _postingAdditionalTransactionRepository;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;

    private const string PaymentWiredToMainMerchantDefaultDescription = "Ödemelerin tamamı ana bayiye yapılmaktadır";
    private const string DefaultCurrency = "TRY";
    private const int HundredPercent = 100;

    public AuthPostAuthBTransReportConsumer(
        ISourceBankAccountService sourceBankAccountService,
        IGenericRepository<PostingBalance> postingBalanceRepository,
        IGenericRepository<PostingTransaction> postingTransactionRepository,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository,
        IGenericRepository<Currency> currencyRepository,
        IGenericRepository<Merchant> merchantRepository,
        ILocationService locationService,
        ILogger<AuthPostAuthBTransReportConsumer> logger,
        IBus bus,
        ICacheService cacheService,
        PfDbContext dbContext, 
        IGenericRepository<PostingAdditionalTransaction> postingAdditionalTransactionRepository, 
        IParameterService parameterService)
    {
        _sourceBankAccountService = sourceBankAccountService;
        _postingBalanceRepository = postingBalanceRepository;
        _postingTransactionRepository = postingTransactionRepository;
        _vposRepository = vposRepository;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
        _currencyRepository = currencyRepository;
        _merchantRepository = merchantRepository;
        _locationService = locationService;
        _logger = logger;
        _bus = bus;
        _cacheService = cacheService;
        _dbContext = dbContext;
        _postingAdditionalTransactionRepository = postingAdditionalTransactionRepository;
        _parameterService = parameterService;
    }

    public async Task Consume(ConsumeContext<AuthPostAuthBTransReport> authReport)
    {
        var sourceBankAccounts = await _cacheService.GetOrCreateAsync("BTransSourceBankAccounts",
            async () => await _sourceBankAccountService.GetAllSourceBankAccountsAsync(
                new GetSourceBankAccountsRequest
                {
                    Source = TransactionSource.PF,
                    AccountType = BankAccountType.UsageAccount,
                    RecordStatus = RecordStatus.Active
                }));
        
        var postingTransactions = await _postingTransactionRepository
            .GetAll()
            .Where(t => authReport.Message.PostingTransactionIds.Contains(t.Id))
            .OrderBy(s => s.CreateDate)
            .ToListAsync();
        
        var postingBalanceIds = postingTransactions.Select(s => s.PostingBalanceId).Distinct().ToList();
        var postingBalances = await 
            _postingBalanceRepository.GetAll()
                .Where(b => postingBalanceIds.Contains(b.Id))
                .Include(t => t.Merchant.Customer.AuthorizedPerson)
                .Include(t => t.Merchant.MerchantBankAccounts)
                .ThenInclude(t => t.Bank)
                .ToListAsync();
        
        var transactionsByBalanceId = postingTransactions.GroupBy(t => t.PostingBalanceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var headMerchantIds = postingBalances.Where(s => s.Merchant.MerchantType == MerchantType.SubMerchant)
            .Select(s => s.Merchant.ParentMerchantId).ToList();

        var headMerchants = await _merchantRepository
            .GetAll()
            .Where(s => headMerchantIds.Contains(s.Id))
            .Include(s => s.Customer)
            .ThenInclude(s => s.AuthorizedPerson)
            .Include(s => s.MerchantBankAccounts)
            .ThenInclude(s => s.Bank)
            .ToListAsync();
        
        var headMerchantCommissionBalanceIds = await _postingAdditionalTransactionRepository.GetAll()
            .Where(s => 
                postingBalanceIds.Contains(s.RelatedPostingBalanceId) && 
                s.TransactionType == TransactionType.ParentMerchantCommission)
            .Select(s => new{BalanceId = s.RelatedPostingBalanceId, CommissionBalanceId = s.PostingBalanceId})
            .Distinct()
            .ToDictionaryAsync(s => s.BalanceId, s => s.CommissionBalanceId);

        var headMerchantCommissionBalances = new List<PostingBalance>();
        if (headMerchantCommissionBalanceIds.Count > 0)
        {
            var headMerchantCommissionBalanceIdsList = headMerchantCommissionBalanceIds.Values.ToList();
            headMerchantCommissionBalances = await _postingBalanceRepository
                .GetAll()
                .Where(s => headMerchantCommissionBalanceIdsList.Contains(s.Id))
                .ToListAsync();
        }
        
        var vposIds = postingTransactions.Where(s => s.VposId != Guid.Empty).Select(a => a.VposId).Distinct().ToList();
        var vposDict = await _vposRepository
            .GetAll()
            .Where(s => vposIds.Contains(s.Id))
            .Include(s => s.AcquireBank.Bank)
            .Include(b => b.VposBankApiInfos
                .Where(a => a.RecordStatus == RecordStatus.Active))
            .ThenInclude(s => s.Key)
            .ToDictionaryAsync(
                v => v.Id,
                v => new
                {
                    MerchantId = v.VposBankApiInfos?.FirstOrDefault(i => i.Key.Category == BankApiKeyCategory.MerchantId)?.Value,
                    TerminalId = v.VposBankApiInfos?.FirstOrDefault(i => i.Key.Category == BankApiKeyCategory.TerminalId)?.Value,
                    AcquireBankName = v.AcquireBank.Bank.Name,
                    AcquireBankCode = v.AcquireBank.Bank.Code.ToString()
                });
        
        var merchantPhysicalPosIds = postingTransactions.Where(s => s.MerchantPhysicalPosId != Guid.Empty)
            .Select(a => a.MerchantPhysicalPosId).Distinct().ToList();
        var merchantPhysicalPosDict = await _merchantPhysicalPosRepository
            .GetAll()
            .Where(s => merchantPhysicalPosIds.Contains(s.Id))
            .Include(s => s.PhysicalPos.AcquireBank.Bank)
            .ToDictionaryAsync(
                v => v.Id,
                v => new
                {
                    MerchantId = v.PosMerchantId,
                    TerminalId = v.PosTerminalId,
                    AcquireBankName = v.PhysicalPos.AcquireBank.Bank.Name,
                    AcquireBankCode = v.PhysicalPos.AcquireBank.Bank.Code.ToString()
                });
        
        var currencyNumbers = postingTransactions.Select(a => a.Currency).Distinct().ToList();
        var currencyDict = await _currencyRepository
            .GetAll()
            .Where(s => currencyNumbers.Contains(s.Number))
            .ToDictionaryAsync(s => s.Number);
        
        var balanceCityCodes = postingBalances.Select(b => b.Merchant.Customer.City).Distinct().ToList();
        var headMerchantCityCodes = headMerchants.Select(s => s.Customer.City).Distinct().ToList();
        var cityCodes = balanceCityCodes.Union(headMerchantCityCodes).ToList();
        var cityLocations = new Dictionary<int, CityWithCountryDto>();
        foreach (var code in cityCodes)
        {
            cityLocations.Add(code, await _cacheService.GetOrCreateAsync(
                $"BTransCityWithCountryDto:{code}",
                async () => await _locationService.GetCountryByCityCode(code)));
        }

        var exceptionList = new List<Guid>();

        var posInformationReportList = new PosInformationReportList{PosInformationReports = [] };
        
        foreach (var postingBalance in postingBalances)
        {
            var bankAccount = postingBalance.Merchant.MerchantBankAccounts.FirstOrDefault(t => t.Iban == postingBalance.Iban) ?? postingBalance.Merchant.MerchantBankAccounts.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

            cityLocations.TryGetValue(postingBalance.Merchant.Customer.City, out var location);
            
            if (!transactionsByBalanceId.TryGetValue(postingBalance.Id, out var balanceTransactions))
            {
                continue;
            }

            var remainingDeductionAmount = authReport.Message.DeductionAmount;

            foreach (var postingTransaction in balanceTransactions)
            {
                decimal deductionToMerchant = 0;
                try
                {
                    var organizationDescription = postingTransaction.OrderId;
                    var receiverIban = bankAccount?.Iban;
                    var receiverBankName = bankAccount?.Bank?.Name;
                    var receiverBankCode = bankAccount?.BankCode.ToString();
                    var receiverWalletNumber = postingBalance.WalletNumber;
                    var relatedBalanceId = postingBalance.Id;
                    
                    var receiverTaxNumber = postingBalance.Merchant.Customer.TaxNumber;
                    var receiverCommercialTitle = postingBalance.Merchant.Customer.CommercialTitle;
                    var receiverIdentityNumber = postingBalance.Merchant.Customer.AuthorizedPerson.IdentityNumber;
                    var receiverFirstName = postingBalance.Merchant.Customer.AuthorizedPerson.Name;
                    var receiverLastName = postingBalance.Merchant.Customer.AuthorizedPerson.Surname;
                    var receiverNationCountryId = location?.Country?.Iso2;
                    var receiverFullAddress = postingBalance.Merchant.Customer.Address;
                    var receiverDistrict = postingBalance.Merchant.Customer.DistrictName;
                    var receiverPostalCode = postingBalance.Merchant.Customer.PostalCode;
                    var receiverCityId = location?.City?.Iso2;
                    var receiverCity = postingBalance.Merchant.Customer.CityName;
                    var receiverPhoneNumber = postingBalance.Merchant.Customer.AuthorizedPerson.MobilePhoneNumber;

                    if (postingTransaction.AmountWithoutCommissions > 0 && remainingDeductionAmount > 0)
                    {
                        if (postingTransaction.AmountWithoutCommissions >= remainingDeductionAmount)
                        {
                            deductionToMerchant = remainingDeductionAmount;
                            remainingDeductionAmount = 0;
                        }
                        else
                        {
                            deductionToMerchant = postingTransaction.AmountWithoutCommissions;
                            remainingDeductionAmount -= postingTransaction.AmountWithoutCommissions;
                        }
                    }
                    
                    var posBankName = ""; 
                    var posBankCode = ""; 
                    var posMerchantId = "";
                    var posTerminalId = "";
                    switch (postingTransaction.PfTransactionSource)
                    {
                        case PfTransactionSource.VirtualPos when postingTransaction.VposId != Guid.Empty:
                        {
                            if (vposDict.TryGetValue(postingTransaction.VposId, out var vposData))
                            {
                                posBankName = vposData.AcquireBankName; 
                                posBankCode = vposData.AcquireBankCode; 
                                posMerchantId = vposData.MerchantId;
                                posTerminalId = vposData.TerminalId;
                            }
                            break;
                        }
                        case PfTransactionSource.PhysicalPos when postingTransaction.MerchantPhysicalPosId != Guid.Empty:
                        {
                            if (merchantPhysicalPosDict.TryGetValue(postingTransaction.MerchantPhysicalPosId, out var physicalPosData))
                            {
                                posBankName = physicalPosData.AcquireBankName; 
                                posBankCode = physicalPosData.AcquireBankCode; 
                                posMerchantId = physicalPosData.MerchantId;
                                posTerminalId = physicalPosData.TerminalId;
                            }
                            break;
                        }
                    }

                    currencyDict.TryGetValue(postingTransaction.Currency, out var currency);

                    var sourceBank = sourceBankAccounts.Items
                        .FirstOrDefault(a =>
                            a.BankCode == postingBalance.MoneyTransferBankCode && a.CurrencyCode == currency?.Code);

                    var headMerchant = headMerchants
                        .FirstOrDefault(s => s.Id == postingBalance.Merchant.ParentMerchantId);
                    var isReceiverSubCompany = postingBalance.Merchant.MerchantType == MerchantType.SubMerchant;
                    var headCompanyTaxNumber = headMerchant?.Customer?.TaxNumber ?? "";
                    var headCompanyCommercialTitle = headMerchant?.Customer?.CommercialTitle ?? "";
                    
                    var totalAmount = postingTransaction.Amount;
                    var netAmount = postingTransaction.AmountWithoutCommissions - deductionToMerchant;
                    var commissionAmount = postingTransaction.PfCommissionAmount + deductionToMerchant;
                    var parentCommissionAmount = postingTransaction.ParentMerchantCommissionAmount;
                    
                    var senderBankName = postingBalance.MoneyTransferBankName;
                    var senderBankCode = postingBalance.MoneyTransferBankCode.ToString();
                    var senderAccountNumber = sourceBank?.AccountNumber;
                    var senderIbanNumber = sourceBank?.IBANNumber;
                    
                    if (postingTransaction.PfCommissionRate + postingTransaction.ParentMerchantCommissionRate == HundredPercent)
                    {
                        string paymentWiredToMainMerchantDescription;
                        try
                        {
                            paymentWiredToMainMerchantDescription =
                                (await _parameterService.GetParametersAsync("PostingParams"))
                                .FirstOrDefault(w => w.ParameterCode == "PaymentWiredToMainMerchantBTransDescription")
                                ?.ParameterValue;
                        }
                        catch
                        {
                            paymentWiredToMainMerchantDescription = PaymentWiredToMainMerchantDefaultDescription;
                        }
                        organizationDescription = (paymentWiredToMainMerchantDescription + "-" + postingTransaction.OrderId).Truncate(300);

                        if (headMerchantCommissionBalanceIds.TryGetValue(postingBalance.Id, out var headMerchantCommissionBalanceId))
                        {
                            var headMerchantCommissionBalance = headMerchantCommissionBalances.FirstOrDefault(s => s.Id == headMerchantCommissionBalanceId);
                            if (headMerchantCommissionBalance != null)
                            {
                                senderBankName = headMerchantCommissionBalance.MoneyTransferBankName;
                                senderBankCode = headMerchantCommissionBalance.MoneyTransferBankCode.ToString();
                                var headMerchantBankAccount = headMerchant?.MerchantBankAccounts?.FirstOrDefault(t => t.Iban == headMerchantCommissionBalance.Iban) 
                                                              ?? headMerchant?.MerchantBankAccounts?.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);
                                var headMerchantSourceBank = sourceBankAccounts.Items
                                    .FirstOrDefault(a =>
                                        a.BankCode == headMerchantCommissionBalance.MoneyTransferBankCode && a.CurrencyCode == currency?.Code);
                                senderAccountNumber = headMerchantSourceBank?.AccountNumber;
                                senderIbanNumber = headMerchantSourceBank?.IBANNumber;
                                receiverIban = headMerchantBankAccount?.Iban;
                                receiverBankName = headMerchantBankAccount?.Bank?.Name;
                                receiverBankCode = headMerchantBankAccount?.BankCode.ToString();
                                receiverWalletNumber = headMerchantCommissionBalance.WalletNumber;
                                relatedBalanceId = headMerchantCommissionBalance.Id;
                            }
                        }
                        
                        var headMerchantLocation = headMerchant?.Customer?.City is { } city && cityLocations.TryGetValue(city, out var headLocation)
                            ? headLocation
                            : location;
                        receiverTaxNumber = headMerchant?.Customer?.TaxNumber;
                        receiverCommercialTitle = headMerchant?.Customer?.CommercialTitle;
                        receiverIdentityNumber = headMerchant?.Customer?.AuthorizedPerson?.IdentityNumber;
                        receiverFirstName = headMerchant?.Customer?.AuthorizedPerson?.Name;
                        receiverLastName = headMerchant?.Customer?.AuthorizedPerson?.Surname;
                        receiverNationCountryId = headMerchantLocation?.Country?.Iso2;
                        receiverFullAddress = headMerchant?.Customer?.Address;
                        receiverDistrict = headMerchant?.Customer?.DistrictName;
                        receiverPostalCode = headMerchant?.Customer?.PostalCode;
                        receiverCityId = headMerchantLocation?.City?.Iso2;
                        receiverCity = headMerchant?.Customer?.CityName;
                        receiverPhoneNumber = headMerchant?.Customer?.AuthorizedPerson?.MobilePhoneNumber;
                        
                        isReceiverSubCompany = false;
                        headCompanyTaxNumber = "";
                        headCompanyCommercialTitle = "";
                        
                        netAmount = postingTransaction.ParentMerchantCommissionAmount;
                        commissionAmount = postingTransaction.PfCommissionAmount;
                        parentCommissionAmount = 0;
                        
                        remainingDeductionAmount += deductionToMerchant;
                    }

                    #region PosInformationReport
                    posInformationReportList.PosInformationReports.Add(new PosInformationReport
                    {
                        OperationType = postingTransaction.PfTransactionSource == PfTransactionSource.VirtualPos ? PosInformationConst.VirtualPos : PosInformationConst.PhysicalPos,
                        RecordType = RecordTypeConst.NewRecord,
                        PosBankName = posBankName,
                        PosBankCode = posBankCode,
                        PosMerchantId = posMerchantId,
                        PosTerminalId = posTerminalId,

                        SenderBankName = senderBankName,
                        SenderBankCode = senderBankCode,
                        SenderAccountNumber = senderAccountNumber,
                        SenderIbanNumber = senderIbanNumber,

                        IsReceiverCustomer = true,
                        IsReceiverCorporate = true,
                        ReceiverTaxNumber = receiverTaxNumber,
                        ReceiverCommercialTitle = receiverCommercialTitle,
                        ReceiverIdentityNumber = receiverIdentityNumber,
                        ReceiverFirstName = receiverFirstName,
                        ReceiverLastName = receiverLastName,
                        ReceiverNationCountryId = receiverNationCountryId,
                        ReceiverFullAddress = receiverFullAddress,
                        ReceiverDistrict = receiverDistrict,
                        ReceiverPostalCode = receiverPostalCode,
                        ReceiverCityId = receiverCityId,
                        ReceiverCity = receiverCity,
                        ReceiverPhoneNumber = receiverPhoneNumber,
                        ReceiverAccountNumber = receiverIban,
                        ReceiverBankName = receiverBankName,
                        ReceiverBankCode = receiverBankCode,
                        ReceiverIbanNumber = receiverIban,
                        ReceiverWalletNumber = receiverWalletNumber,
                        
                        CurrencyCode = currency?.Code ?? DefaultCurrency,
                        RelatedTransactionId = postingTransaction.MerchantTransactionId,
                        TransactionDate = postingTransaction.TransactionDate,
                        PaymentDate = postingTransaction.PaymentDate,
                        NetAmount = netAmount,
                        ConvertedAmount = netAmount,
                        Amount = totalAmount,
                        TotalPricingAmount = commissionAmount,
                        OrganizationDescription = organizationDescription,
                        
                        IsReceiverSubCompany = isReceiverSubCompany,
                        HeadCompanyTaxNumber = headCompanyTaxNumber,
                        HeadCompanyCommercialTitle = headCompanyCommercialTitle,
                        HeadCompanyPricingAmount = parentCommissionAmount,
                        
                        RelatedBalanceId = relatedBalanceId,
                        RelatedInstallmentTransactionId = postingTransaction.MerchantInstallmentTransactionId
                    });
                    #endregion
                }
                catch (Exception exception)
                {
                    remainingDeductionAmount += deductionToMerchant;
                    exceptionList.Add(postingTransaction.Id);
                    _logger.LogError($"PostingTransaction({postingTransaction.Id}) btrans report failed {exception}");
                }
            }
        }
        
        try
        {
            if (posInformationReportList.PosInformationReports.Count > 0)
            {
                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.SavePosInformation"));
                await endpoint.Send(posInformationReportList, tokenSource.Token);
                
                var now = DateTime.Now;
                var validIds = authReport.Message.PostingTransactionIds
                    .Except(exceptionList)
                    .ToList();
                await _dbContext.PostingTransaction
                    .Where(t => validIds.Contains(t.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(b => b.BTransStatus, _ => PostingBTransStatus.Completed)
                        .SetProperty(b => b.UpdateDate, _ => now));
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"Btrans report sending to queue failed {exception}");
        }
    }
}