using LinkPara.HttpProviders.BTrans;
using LinkPara.HttpProviders.BTrans.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.BTrans;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TransactionSource = LinkPara.SharedModels.Banking.Enums.TransactionSource;

namespace LinkPara.PF.Infrastructure.Consumers;

public class ReturnBTransReportConsumer : IConsumer<ReturnBTransReport>
{
    private readonly ISourceBankAccountService _sourceBankAccountService;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILocationService _locationService;
    private readonly IBus _bus;
    private readonly IBTransPosInformationService _bTransPosInformationService;
    private readonly IGenericRepository<PostingAdditionalTransaction> _postingAdditionalTransactionRepository;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;

    private const string PaymentWiredToMainMerchantDefaultDescription = "Ödemelerin tamamı ana bayiye yapılmaktadır";
    private const string DefaultCurrency = "TRY";
    private const int HundredPercent = 100;

    public ReturnBTransReportConsumer(
        ISourceBankAccountService sourceBankAccountService,
        IGenericRepository<PostingBalance> postingBalanceRepository,
        IGenericRepository<PostingTransaction> postingTransactionRepository,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<Currency> currencyRepository,
        IGenericRepository<Merchant> merchantRepository,
        ILocationService locationService,
        IBus bus,
        IBTransPosInformationService bTransPosInformationService,
        IGenericRepository<PostingAdditionalTransaction> postingAdditionalTransactionRepository,
        IParameterService parameterService,
        IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository)
    {
        _sourceBankAccountService = sourceBankAccountService;
        _postingBalanceRepository = postingBalanceRepository;
        _postingTransactionRepository = postingTransactionRepository;
        _vposRepository = vposRepository;
        _currencyRepository = currencyRepository;
        _merchantRepository = merchantRepository;
        _locationService = locationService;
        _bus = bus;
        _bTransPosInformationService = bTransPosInformationService;
        _postingAdditionalTransactionRepository = postingAdditionalTransactionRepository;
        _parameterService = parameterService;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
    }

    public async Task Consume(ConsumeContext<ReturnBTransReport> returnReport)
    {
        var sourceBankAccounts = await _sourceBankAccountService.GetAllSourceBankAccountsAsync(
            new GetSourceBankAccountsRequest
            {
                Source = TransactionSource.PF,
                AccountType = BankAccountType.UsageAccount,
                RecordStatus = RecordStatus.Active
            });

        var referencePostingTransaction = await _postingTransactionRepository
            .GetAll()
            .Where(t => t.Id == returnReport.Message.ReferencePostingTransactionId)
            .FirstOrDefaultAsync();

        if (referencePostingTransaction is null || referencePostingTransaction.BTransStatus != PostingBTransStatus.Completed)
        {
            return;
        }

        var returnPostingTransactions = await _postingTransactionRepository
            .GetAll()
            .Where(t => returnReport.Message.ReturnPostingTransactionIds.Contains(t.Id))
            .OrderBy(s => s.CreateDate)
            .ToListAsync();

        var referencePostingBalance = await _postingBalanceRepository
            .GetAll()
            .Where(s => s.Id == referencePostingTransaction.PostingBalanceId)
            .Include(s => s.Merchant)
            .ThenInclude(a => a.MerchantBankAccounts)
            .ThenInclude(b => b.Bank)
            .Include(a => a.Merchant.Customer)
            .ThenInclude(s => s.AuthorizedPerson)
            .FirstOrDefaultAsync();
        
        if (referencePostingBalance is null)
        {
            return;
        }

        var posBankName = "";
        var posBankCode = "";
        var posMerchantId = "";
        var posTerminalId = "";
        switch (referencePostingTransaction.PfTransactionSource)
        {
            case PfTransactionSource.VirtualPos when referencePostingTransaction.VposId != Guid.Empty:
            {
                var vpos = await _vposRepository
                    .GetAll()
                    .Where(s => s.Id == referencePostingTransaction.VposId)
                    .Include(s => s.AcquireBank.Bank)
                    .Include(b => b.VposBankApiInfos
                        .Where(a => a.RecordStatus == RecordStatus.Active))
                    .ThenInclude(s => s.Key)
                    .FirstOrDefaultAsync();
                if (vpos != null)
                {
                    posMerchantId =
                        vpos.VposBankApiInfos?.FirstOrDefault(s => s.Key.Category == BankApiKeyCategory.MerchantId)?.Value;
                    posTerminalId =
                        vpos.VposBankApiInfos?.FirstOrDefault(s => s.Key.Category == BankApiKeyCategory.TerminalId)?.Value;
                    posBankName = vpos.AcquireBank.Bank.Name;
                    posBankCode = vpos.AcquireBank.Bank.Code.ToString();
                }
                break;
            }
            case PfTransactionSource.PhysicalPos when referencePostingTransaction.MerchantPhysicalPosId != Guid.Empty:
            {
                var merchantPhysicalPos = await _merchantPhysicalPosRepository
                    .GetAll()
                    .Where(s => s.Id == referencePostingTransaction.MerchantPhysicalPosId)
                    .Include(s => s.PhysicalPos.AcquireBank.Bank)
                    .FirstOrDefaultAsync();
                if (merchantPhysicalPos != null)
                {
                    posBankName = merchantPhysicalPos.PhysicalPos.AcquireBank.Bank.Name;
                    posBankCode = merchantPhysicalPos.PhysicalPos.AcquireBank.Bank.Code.ToString();
                    posMerchantId = merchantPhysicalPos.PosMerchantId;
                    posTerminalId = merchantPhysicalPos.PosTerminalId;
                }
                break;
            }
        }

        var currency = await _currencyRepository
            .GetAll()
            .Where(s => s.Number == referencePostingTransaction.Currency)
            .FirstOrDefaultAsync();

        var bankAccount =
            referencePostingBalance.Merchant.MerchantBankAccounts.FirstOrDefault(t =>
                t.Iban == referencePostingBalance.Iban) ??
            referencePostingBalance.Merchant.MerchantBankAccounts.FirstOrDefault(s =>
                s.RecordStatus == RecordStatus.Active);

        var location = await _locationService.GetCountryByCityCode(referencePostingBalance.Merchant.Customer.City);

        var relatedTransactionId = referencePostingTransaction.MerchantTransactionId;
        var relatedMerchantInstallmentTransactionId = referencePostingTransaction.MerchantInstallmentTransactionId;

        var sourceBank = sourceBankAccounts.Items
            .FirstOrDefault(a =>
                a.BankCode == referencePostingBalance.MoneyTransferBankCode && a.CurrencyCode == currency?.Code);

        var senderBankName = referencePostingBalance.MoneyTransferBankName;
        var senderBankCode = referencePostingBalance.MoneyTransferBankCode.ToString();
        var senderAccountNumber = sourceBank?.AccountNumber;
        var senderIbanNumber = sourceBank?.IBANNumber;

        var totalRefundAmount = returnPostingTransactions.Sum(s => s.Amount);

        var totalNetAmount = referencePostingTransaction.AmountWithoutCommissions -
                             returnPostingTransactions.Sum(s => s.AmountWithoutCommissions);

        var totalAmount = referencePostingTransaction.Amount - returnPostingTransactions.Sum(s => s.Amount);

        var totalPricingAmount = referencePostingTransaction.PfCommissionAmount -
                                 returnPostingTransactions.Sum(s => s.PfCommissionAmount);

        var totalParentMerchantPricingAmount = referencePostingTransaction.ParentMerchantCommissionAmount -
                                               returnPostingTransactions.Sum(s => s.ParentMerchantCommissionAmount);

        var headMerchant = await _merchantRepository
            .GetAll()
            .Where(s => s.Id == referencePostingBalance.Merchant.ParentMerchantId)
            .Include(s => s.Customer)
            .ThenInclude(s => s.AuthorizedPerson)
            .Include(s => s.MerchantBankAccounts)
            .ThenInclude(s => s.Bank)
            .FirstOrDefaultAsync();

        var isReceiverSubCompany = referencePostingBalance.Merchant.MerchantType == MerchantType.SubMerchant;
        var headCompanyTaxNumber = headMerchant?.Customer?.TaxNumber ?? "";
        var headCompanyCommercialTitle = headMerchant?.Customer?.CommercialTitle ?? "";

        var organizationDescription = referencePostingTransaction.OrderId;
        var receiverIban = bankAccount?.Iban;
        var receiverBankName = bankAccount?.Bank?.Name;
        var receiverBankCode = bankAccount?.BankCode.ToString();
        var receiverWalletNumber = referencePostingBalance.WalletNumber;
        var relatedBalanceId = referencePostingBalance.Id;

        var receiverTaxNumber = referencePostingBalance.Merchant.Customer.TaxNumber;
        var receiverCommercialTitle = referencePostingBalance.Merchant.Customer.CommercialTitle;
        var receiverIdentityNumber = referencePostingBalance.Merchant.Customer.AuthorizedPerson.IdentityNumber;
        var receiverFirstName = referencePostingBalance.Merchant.Customer.AuthorizedPerson.Name;
        var receiverLastName = referencePostingBalance.Merchant.Customer.AuthorizedPerson.Surname;
        var receiverNationCountryId = location?.Country?.Iso2;
        var receiverFullAddress = referencePostingBalance.Merchant.Customer.Address;
        var receiverDistrict = referencePostingBalance.Merchant.Customer.DistrictName;
        var receiverPostalCode = referencePostingBalance.Merchant.Customer.PostalCode;
        var receiverCityId = location?.City?.Iso2;
        var receiverCity = referencePostingBalance.Merchant.Customer.CityName;
        var receiverPhoneNumber = referencePostingBalance.Merchant.Customer.AuthorizedPerson.MobilePhoneNumber;

        if (referencePostingTransaction.PfCommissionRate + referencePostingTransaction.ParentMerchantCommissionRate == HundredPercent)
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

            organizationDescription =
                (paymentWiredToMainMerchantDescription + "-" + referencePostingTransaction.OrderId).Truncate(300);

            var headMerchantCommissionBalanceId = await _postingAdditionalTransactionRepository.GetAll()
                .Where(s =>
                    s.RelatedPostingBalanceId == referencePostingBalance.Id &&
                    s.TransactionType == TransactionType.ParentMerchantCommission)
                .Select(s => s.PostingBalanceId)
                .FirstOrDefaultAsync();

            var headMerchantCommissionBalance = await _postingBalanceRepository
                .GetAll()
                .Where(s => s.Id == headMerchantCommissionBalanceId)
                .FirstOrDefaultAsync();
            if (headMerchantCommissionBalance != null)
            {
                var headMerchantBankAccount =
                    headMerchant?.MerchantBankAccounts.FirstOrDefault(t =>
                        t.Iban == headMerchantCommissionBalance.Iban) ??
                    headMerchant?.MerchantBankAccounts.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

                var headMerchantLocation = headMerchant?.Customer?.City is { } city
                    ? await _locationService.GetCountryByCityCode(city)
                    : location;

                var headMerchantSourceBank = sourceBankAccounts.Items
                    .FirstOrDefault(a =>
                        a.BankCode == headMerchantCommissionBalance.MoneyTransferBankCode &&
                        a.CurrencyCode == currency?.Code);

                senderBankName = headMerchantCommissionBalance.MoneyTransferBankName;
                senderBankCode = headMerchantCommissionBalance.MoneyTransferBankCode.ToString();
                senderAccountNumber = headMerchantSourceBank?.AccountNumber;
                senderIbanNumber = headMerchantSourceBank?.IBANNumber;

                receiverIban = headMerchantBankAccount?.Iban;
                receiverBankName = headMerchantBankAccount?.Bank?.Name;
                receiverBankCode = headMerchantBankAccount?.BankCode.ToString();
                receiverWalletNumber = headMerchantCommissionBalance.WalletNumber;
                relatedBalanceId = headMerchantCommissionBalance.Id;

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

                totalNetAmount = totalParentMerchantPricingAmount;
                totalParentMerchantPricingAmount = 0;
            }
        }

        #region PosInformationReport

        var posInformationReport = new PosInformationReport
        {
            OperationType = referencePostingTransaction.PfTransactionSource == PfTransactionSource.VirtualPos
                ? PosInformationConst.VirtualPos
                : PosInformationConst.PhysicalPos,
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
            RelatedTransactionId = relatedTransactionId,
            TransactionDate = referencePostingTransaction.TransactionDate,
            PaymentDate = referencePostingBalance.PaymentDate,
            NetAmount = totalNetAmount,
            ConvertedAmount = totalNetAmount,
            Amount = totalAmount,
            TotalPricingAmount = totalPricingAmount,
            OrganizationDescription = organizationDescription,

            IsReceiverSubCompany = isReceiverSubCompany,
            HeadCompanyTaxNumber = headCompanyTaxNumber,
            HeadCompanyCommercialTitle = headCompanyCommercialTitle,
            HeadCompanyPricingAmount = totalParentMerchantPricingAmount,

            RelatedBalanceId = relatedBalanceId,
            RelatedInstallmentTransactionId = relatedMerchantInstallmentTransactionId
        };

        #endregion

        if (referencePostingTransaction.Amount <= totalRefundAmount)
        {
            await SendDeletePosInformationRecordAsync(relatedTransactionId, relatedMerchantInstallmentTransactionId, RecordTypeConst.CancelRecord);
        }
        else
        {
            await _bTransPosInformationService.DeletePosInformationRecordAsync(
                new DeletePosInformationRecordRequest
                    { RelatedTransactionId = relatedTransactionId, RelatedInstallmentTransactionId = relatedMerchantInstallmentTransactionId, RecordType = RecordTypeConst.DeleteRecord }
            );

            if (totalAmount > 0)
            {
                await SavePosInformationReportAsync(posInformationReport);
            }
        }

        returnPostingTransactions.ForEach(s =>
        {
            s.BTransStatus = PostingBTransStatus.Completed;
            s.UpdateDate = DateTime.Now;
        });
        await _postingTransactionRepository.UpdateRangeAsync(returnPostingTransactions);
    }

    private async Task SavePosInformationReportAsync(PosInformationReport posInformationReport)
    {
        var posInformationReportList = new PosInformationReportList
        {
            PosInformationReports =
            [
                posInformationReport
            ]
        };

        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.SavePosInformation"));
        await endpoint.Send(posInformationReportList, tokenSource.Token);
    }

    private async Task SendDeletePosInformationRecordAsync(Guid relatedTransactionId, Guid relatedMerchantInstallmentTransactionId, string recordType)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.DeletePosInformationRecord"));
        await endpoint.Send(new DeletePosInformationRecord
        {
            RecordType = recordType,
            RelatedTransactionId = relatedTransactionId,
            RelatedInstallmentTransactionId = relatedMerchantInstallmentTransactionId
        }, tokenSource.Token);
    }
}