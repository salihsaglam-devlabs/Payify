using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Commons.Models.LogoConfiguration;
using LinkPara.Accounting.Application.Commons.Models.LogoRequests.CreateCustomer;
using LinkPara.Accounting.Application.Commons.Models.LogoRequests.SavePayment;
using LinkPara.Accounting.Application.Commons.Models.LogoResponse;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.Accounting.Domain.Enums;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using LinkPara.Accounting.Application.Commons.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.Accounting.Application.Commons.Models.DefaultCustomerConfiguration;
using LinkPara.Accounting.Application.Commons.Models;
using LinkPara.Security;
using System.Text;
using System;
using System.Web;

namespace LinkPara.Accounting.Infrastructure.Services.AccountingServices;

public class TigerAccountingService : IAccountingService
{
    private readonly HttpClient _client;
    private readonly IGenericRepository<ExternalCurrency> _currencyRepository;
    private readonly IGenericRepository<BankAccount> _bankAccount;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IGenericRepository<Template> _templateRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TigerAccountingService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IVaultClient _vaultClient;
    private readonly IBus _bus;
    private readonly IParameterService _parameterService;
    private readonly ISecureRandomGenerator _randomGenerator;

    private const int MaxResultMessageSize = 800;

    public TigerAccountingService(HttpClient client,
                             IGenericRepository<ExternalCurrency> currencyRepository,
                             IGenericRepository<Payment> paymentRepository,
                             IGenericRepository<Template> templateRepository,
                             ICacheService cacheService,
                             ILogger<TigerAccountingService> logger,
                             IGenericRepository<BankAccount> bankAccount,
                             IGenericRepository<Customer> customerRepository,
                             IAuditLogService auditLogService,
                             IVaultClient vaultClient,
                             IBus bus,
                             IParameterService parameterService,
                             ISecureRandomGenerator randomGenerator)
    {
        _client = client;
        _currencyRepository = currencyRepository;
        _paymentRepository = paymentRepository;
        _templateRepository = templateRepository;
        _cacheService = cacheService;
        _logger = logger;
        _bankAccount = bankAccount;
        _customerRepository = customerRepository;
        _auditLogService = auditLogService;
        _vaultClient = vaultClient;
        _bus = bus;
        _parameterService = parameterService;
        _randomGenerator = randomGenerator;
    }

    public async Task CreateCustomerAsync(AccountingCustomer customerRequest)
    {
        var dbCustomer = await _customerRepository.GetAll().FirstOrDefaultAsync(x => x.Code == customerRequest.Code);
        if (dbCustomer is not null)
        {
            await RetryPostCustomerAsync(dbCustomer);
            return;
        }

        FillEmptyFieldsWithDefaultInformation(customerRequest);

        var customer = new Customer
        {
            Code = customerRequest.Code,
            FirstName = customerRequest.FirstName,
            LastName = customerRequest.LastName,
            Email = customerRequest.Email,
            PhoneNumber = customerRequest.PhoneNumber,
            PhoneCode = customerRequest.PhoneCode,
            IdentityNumber = customerRequest.IdentityNumber,
            Title = customerRequest.Title,
            CurrencyCode = customerRequest.CurrencyCode,
            CreatedBy = customerRequest.UserId.ToString(),
            AccountingCustomerType = customerRequest.AccountingCustomerType,
            City = customerRequest.City,
            CityCode = customerRequest.CityCode,
            Country = customerRequest.Country,
            CountryCode = customerRequest.CountryCode,
            Address = customerRequest.Address,
            TaxOffice = customerRequest.TaxOffice,
            TaxNumber = customerRequest.TaxNumber,
            TaxOfficeCode = customerRequest.TaxOfficeCode,
            Town = customerRequest.Town,
            District = customerRequest.District,
            CustomerCode = customerRequest.CustomerCode,
            AccountingCustomerCode = customerRequest.CustomerCode
        };

        try
        {
            var customerResponse = await PostCustomerAsync(customer);

            customer.IsSuccess = customerResponse.isSuccess;
            customer.ResultMessage = customerResponse.strMesage.Length > MaxResultMessageSize
                ? customerResponse.strMesage.Substring(0, MaxResultMessageSize)
                : customerResponse.strMesage;

            _logger.LogError($"Accounting Logo Customer Error : {customerResponse.strMesage}");

            await _customerRepository.AddAsync(customer);

            await SaveCustomerAuditLogAsync(customer);
        }
        catch (Exception exception)
        {
            customer.IsSuccess = false;
            customer.ResultMessage = exception.Message.Length > MaxResultMessageSize
                                            ? exception.Message.Substring(0, MaxResultMessageSize)
                                            : exception.Message;
            await _customerRepository.AddAsync(customer);

            await SaveCustomerAuditLogAsync(customer);

            _logger.LogError($"ErrorPostCustomer Error: {exception}");
        }
    }

    private void FillEmptyFieldsWithDefaultInformation(AccountingCustomer customerRequest)
    {
        if (customerRequest.AccountingCustomerType == AccountingCustomerType.Emoney)
        {
            var defaultCustomerInformation = _vaultClient.GetSecretValue<DefaultCustomerInformation>("AccountingSecrets", "DefaultCustomerInfo");
            customerRequest.TaxOffice = string.IsNullOrWhiteSpace(customerRequest.TaxOffice)
                ? defaultCustomerInformation.TaxOffice
                : customerRequest.TaxOffice;
            customerRequest.TaxOfficeCode = string.IsNullOrWhiteSpace(customerRequest.TaxOfficeCode)
                ? defaultCustomerInformation.TaxOfficeCode
                : customerRequest.TaxOfficeCode;
            customerRequest.TaxNumber = string.IsNullOrWhiteSpace(customerRequest.TaxNumber)
                ? defaultCustomerInformation.TaxNumber
                : customerRequest.TaxNumber;
            customerRequest.IdentityNumber = string.IsNullOrWhiteSpace(customerRequest.IdentityNumber)
                ? defaultCustomerInformation.IdentityNumber
                : customerRequest.IdentityNumber;
            customerRequest.Address = string.IsNullOrWhiteSpace(customerRequest.Address)
                ? defaultCustomerInformation.Address
                : customerRequest.Address;
            customerRequest.Country = string.IsNullOrWhiteSpace(customerRequest.Country)
                ? defaultCustomerInformation.Country
                : customerRequest.Country;
            customerRequest.CountryCode = string.IsNullOrWhiteSpace(customerRequest.CountryCode)
                ? defaultCustomerInformation.CountryCode
                : customerRequest.CountryCode;
            customerRequest.CityCode = string.IsNullOrWhiteSpace(customerRequest.CityCode)
                ? defaultCustomerInformation.CityCode
                : customerRequest.CityCode;
            customerRequest.City = string.IsNullOrWhiteSpace(customerRequest.City)
                ? defaultCustomerInformation.City
                : customerRequest.City;
            customerRequest.District = string.IsNullOrWhiteSpace(customerRequest.District)
                ? defaultCustomerInformation.District
                : customerRequest.District;
            customerRequest.Town = string.IsNullOrWhiteSpace(customerRequest.Town)
                ? defaultCustomerInformation.Town
                : customerRequest.Town;
        }
    }

    public async Task CancelPaymentAsync(Guid clientReferenceId)
    {
        var dbPayment = await _paymentRepository.GetAll()
            .FirstOrDefaultAsync(x => x.ClientReferenceId == clientReferenceId);

        if (dbPayment is null)
        {
            throw new NotFoundException(nameof(dbPayment));
        }

        if (!dbPayment.IsSuccess)
        {
            throw new CanNotCancelNotSuccessPaymentException();
        }

        var templateLine = await _templateRepository.GetAll()
            .FirstOrDefaultAsync(s => s.OperationType == dbPayment.OperationType
                        && s.HasCommission == dbPayment.HasCommission);

        if (templateLine is null)
        {
            throw new NotFoundException(nameof(templateLine));
        }

        try
        {
            var cancelPaymentResponse = await DeletePaymentAsync("api/payment", dbPayment.ReferenceId, templateLine.ExternalOperationType);

            if (!cancelPaymentResponse.IsSuccessStatusCode)
            {
                var exceptionDetails = await cancelPaymentResponse.Content.ReadAsStringAsync();
                dbPayment.IsSuccess = false;
                await CancelPaymentAuditLogAsync(dbPayment);
                throw new Exception(exceptionDetails);
            }

            var serviceResponse = await cancelPaymentResponse.Content.ReadFromJsonAsync<ServiceResponse>();

            await CancelPaymentAuditLogAsync(dbPayment);
            dbPayment.IsCanceled = serviceResponse.isSuccess;
            dbPayment.CancelResultMessage = serviceResponse.strMesage.Length > MaxResultMessageSize
                                            ? serviceResponse.strMesage.Substring(0, MaxResultMessageSize)
                                            : serviceResponse.strMesage;
            await _paymentRepository.UpdateAsync(dbPayment);
        }
        catch (Exception exception)
        {
            dbPayment.IsCanceled = false;
            dbPayment.CancelResultMessage = exception.Message.Length > MaxResultMessageSize
                                            ? exception.Message.Substring(0, MaxResultMessageSize)
                                            : exception.Message;
            await _paymentRepository.UpdateAsync(dbPayment);

            await CancelPaymentAuditLogAsync(dbPayment);

            _logger.LogError($"ErrorCancelPayment Error: {exception}");

            throw;
        }
    }

    private async Task CancelPaymentAuditLogAsync(Payment payment)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = payment.IsSuccess,
                LogDate = DateTime.Now,
                Operation = "CancelPayment",
                SourceApplication = "Accounting",
                Resource = "Payment",
                Details = new Dictionary<string, string>
                {
                    { "UserId", payment.CreatedBy },
                    { "TransactionDate", payment.TransactionDate.ToString() },
                    { "OperationType", payment.OperationType.ToString() },
                }
            });
    }

    public async Task PostPaymentAsync(AccountingPayment paymentRequest)
    {
        if (!string.IsNullOrWhiteSpace(paymentRequest.ReferenceId))
        {
            await RetryPaymentAsync(paymentRequest);
            return;
        }

        var referenceId = await GenerateReferenceIdAsync();

        var payment = new Payment
        {
            ReferenceId = referenceId,
            Amount = paymentRequest.Amount,
            BsmvAmount = paymentRequest.BsmvAmount,
            CommissionAmount = paymentRequest.CommissionAmount,
            ReceiverBsmvAmount = paymentRequest.ReceiverBsmvAmount,
            ReceiverCommissionAmount = paymentRequest.ReceiverCommissionAmount,
            CurrencyCode = paymentRequest.CurrencyCode,
            Destination = paymentRequest.Destination,
            HasCommission = paymentRequest.HasCommission,
            IsSuccess = false,
            OperationType = paymentRequest.OperationType,
            Source = paymentRequest.Source,
            TransactionDate = paymentRequest.TransactionDate,
            CreatedBy = paymentRequest.UserId.ToString(),
            AccountingTransactionType = paymentRequest.AccountingTransactionType,
            BankCode = paymentRequest.BankCode,
            ClientReferenceId = paymentRequest.ClientReferenceId,
            AccountingCustomerType = paymentRequest.AccountingCustomerType,
            TransactionId = paymentRequest.TransactionId.ToString(),
            MerchantId = paymentRequest.MerchantId,
            ReturnAmount = paymentRequest.ReturnAmount,
            BankCommissionAmount = paymentRequest.BankCommissionAmount,
            ChargebackAmount = paymentRequest.ChargebackAmount,
            SuspiciousAmount = paymentRequest.SuspiciousAmount,
            DueAmount = paymentRequest.DueAmount,
            ChargebackReturnAmount = paymentRequest.ChargebackReturnAmount,
            SuspiciousReturnAmount = paymentRequest.SuspiciousReturnAmount
        };

        try
        {
            var paymentResponse = await SendPaymentAsync(payment);
            payment.IsSuccess = paymentResponse.isSuccess;
            payment.ResultMessage = paymentResponse.strMesage.Length > MaxResultMessageSize
                ? paymentResponse.strMesage.Substring(0, MaxResultMessageSize)
                : paymentResponse.strMesage;

            await _paymentRepository.AddAsync(payment);
            await SavePaymentAuditLogAsync(payment);
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                                            ? exception.Message.Substring(0, MaxResultMessageSize)
                                            : exception.Message;

            await _paymentRepository.AddAsync(payment);
            await SavePaymentAuditLogAsync(payment);

            _logger.LogError($"ErrorOnSavePayment Error: {exception}");
        }
    }

    private async Task SaveCustomerAuditLogAsync(Customer customerResult)
    {
        await _auditLogService.AuditLogAsync(
                        new AuditLog
                        {
                            IsSuccess = customerResult.IsSuccess,
                            LogDate = DateTime.Now,
                            Operation = "CreateCustomer",
                            SourceApplication = "Accounting",
                            Resource = "Customer",
                            Details = new Dictionary<string, string>
                            {
                                { "UserId", customerResult.CreatedBy.ToString() },
                                { "FirstName", customerResult.FirstName },
                                { "Email", customerResult.Email }
                            }
                        });
    }

    private async Task RetryPostCustomerAsync(Customer dbCustomer)
    {
        if (dbCustomer.IsSuccess)
        {
            throw new CanNotRetrySuccessCustomerException();
        }

        try
        {
            var customerResponse = await PostCustomerAsync(dbCustomer);

            dbCustomer.IsSuccess = customerResponse.isSuccess;
            dbCustomer.ResultMessage = customerResponse.strMesage.Length > MaxResultMessageSize
                                            ? customerResponse.strMesage.Substring(0, MaxResultMessageSize)
                                            : customerResponse.strMesage;

            await _customerRepository.UpdateAsync(dbCustomer);
            await SaveCustomerAuditLogAsync(dbCustomer);
        }
        catch (Exception exception)
        {
            dbCustomer.IsSuccess = false;
            dbCustomer.ResultMessage = exception.Message.Length > MaxResultMessageSize
                                            ? exception.Message.Substring(0, MaxResultMessageSize)
                                            : exception.Message;

            await _customerRepository.UpdateAsync(dbCustomer);
            await SaveCustomerAuditLogAsync(dbCustomer);

            _logger.LogError($"ErrorRetryPostCustomer Error: {exception}");
        }
        if (!dbCustomer.IsSuccess)
        {
            throw new RetryFailedException();
        }
    }

    private async Task<ServiceResponse> PostCustomerAsync(Customer customerResult)
    {
        var currency = await GetExternalCurrencyFromCacheAsync(customerResult.CurrencyCode, customerResult.AccountingCustomerType);

        var createCustomerRequest = new CreateCustomerRequest
        {
            SOURCE_ID = customerResult.Code,
            CODE = string.Empty,
            NAME = customerResult.FirstName,
            SURNAME = customerResult.LastName,
            E_MAIL = customerResult.Email,
            TELEPHONE1 = customerResult.PhoneNumber,
            TELEPHONE1_CODE = customerResult.PhoneCode,
            TCKNO = customerResult.IdentityNumber,
            TITLE = customerResult.Title,
            CURRENCY = currency.ExternalCurrencyId,
            GL_CODE = currency.AccountCode,
            CITY = customerResult.City,
            CITY_CODE = customerResult.CityCode,
            COUNTRY = customerResult.Country,
            COUNTRY_CODE = customerResult.CountryCode,
            ADDRESS1 = customerResult.Address,
            TAX_ID = customerResult.TaxNumber,
            TAX_OFFICE = customerResult.TaxOffice,
            TAX_OFFICE_CODE = customerResult.TaxOfficeCode,
            TOWN = customerResult.District,
            DISTRICT = customerResult.Town
        };

        var response = await PostAsJsonAsync("api/Customer", createCustomerRequest);

        if (!response.IsSuccessStatusCode)
        {
            var exceptionDetails = await response.Content.ReadAsStringAsync();
            throw new Exception(exceptionDetails);
        }

        var customerResponse = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        return customerResponse;
    }

    private async Task SavePaymentAuditLogAsync(Payment payment)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = payment.IsSuccess,
                LogDate = DateTime.Now,
                Operation = "PostPayment",
                SourceApplication = "Accounting",
                Resource = "Payment",
                Details = new Dictionary<string, string>
                {
                    { "UserId", payment.CreatedBy },
                    { "TransactionDate", payment.TransactionDate.ToString() },
                    { "OperationType", payment.OperationType.ToString() },
                }
            });
    }

    private async Task<ServiceResponse> SendPaymentAsync(Payment payment)
    {
        var templateLines = await GetTemplateLinesAsync(payment);

        var paymentLines = await PreparePaymentLinesAsync(templateLines, payment);

        var paymentRequest = new SavePaymentRequest
        {
            DATE_ = payment.TransactionDate,
            REFERANCE = payment.ReferenceId,
            OPR_TYPE = templateLines.FirstOrDefault()!.ExternalOperationType,
            lines = paymentLines
        };

        var response = await PostAsJsonAsync("api/Payment", paymentRequest);

        if (!response.IsSuccessStatusCode)
        {
            var exceptionDetails = await response.Content.ReadAsStringAsync();
            throw new Exception(exceptionDetails);
        }

        var paymentResponse = await response.Content.ReadFromJsonAsync<ServiceResponse>();

        if (payment.OperationType == OperationType.PfBalance)
        {
            var merchantId = _vaultClient.GetSecretValue<Guid?>("SharedSecrets", "PaymentProviderConfigs", "MerchantId");
            if (payment.MerchantId == merchantId)
            {
                var templateLinesForPfCardTopupBalance = await _templateRepository.GetAll()
                        .Where(s => s.OperationType == OperationType.PfCardTopupBalance
                            && s.HasCommission == payment.HasCommission
                            && s.RecordStatus == RecordStatus.Active).ToListAsync();

                if (templateLinesForPfCardTopupBalance.Any())
                {
                    payment.OperationType = OperationType.PfCardTopupBalance;
                    var paymentLinesForPfCardTopupBalance = await PreparePaymentLinesAsync(templateLinesForPfCardTopupBalance, payment);
                    payment.OperationType = OperationType.PfBalance;
                    var paymentRequestForPfCardTopupBalance = new SavePaymentRequest
                    {
                        DATE_ = payment.TransactionDate,
                        REFERANCE = payment.ReferenceId,
                        OPR_TYPE = templateLinesForPfCardTopupBalance.FirstOrDefault()!.ExternalOperationType,
                        lines = paymentLinesForPfCardTopupBalance
                    };

                    var responseForPfCardTopupBalance = await PostAsJsonAsync("api/Payment", paymentRequestForPfCardTopupBalance);

                    if (!responseForPfCardTopupBalance.IsSuccessStatusCode)
                    {
                        var exceptionDetails = await responseForPfCardTopupBalance.Content.ReadAsStringAsync();
                        _logger.LogError($"SendPaymentAsync Error: {exceptionDetails}");
                    }
                }
            }
        }

        return paymentResponse;
    }

    private async Task RetryPaymentAsync(AccountingPayment payment)
    {
        var dbPayment = await _paymentRepository.GetAll()
            .FirstOrDefaultAsync(x => x.ReferenceId == payment.ReferenceId);

        if (dbPayment is null)
        {
            throw new NotFoundException(nameof(dbPayment));
        }

        if (dbPayment.IsSuccess)
        {
            throw new CanNotRetrySuccessPaymentException();
        }

        dbPayment.FailedPaymentRetryCount++;

        try
        {
            var paymentResponse = await SendPaymentAsync(dbPayment);
            dbPayment.IsSuccess = paymentResponse.isSuccess;
            dbPayment.ResultMessage = paymentResponse.strMesage.Length > MaxResultMessageSize
                                            ? paymentResponse.strMesage.Substring(0, MaxResultMessageSize)
                                            : paymentResponse.strMesage;

            await _paymentRepository.UpdateAsync(dbPayment);
            await SavePaymentAuditLogAsync(dbPayment);
        }
        catch (Exception exception)
        {
            dbPayment.IsSuccess = false;
            dbPayment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                                            ? exception.Message.Substring(0, MaxResultMessageSize)
                                            : exception.Message;

            await _paymentRepository.UpdateAsync(dbPayment);
            await SavePaymentAuditLogAsync(dbPayment);

            _logger.LogError($"ErrorRetrySavePayment Error: {exception}");
        }
        if (!dbPayment.IsSuccess)
        {
            throw new RetryFailedException();
        }
    }

    private async Task<List<PaymentLine>> PreparePaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        if (payment.HasCommission
            && payment.OperationType == OperationType.EmoneyTransfer)
        {
            return await GetP2PWithCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
                 && payment.OperationType == OperationType.EmoneyTransfer)
        {
            return await GetP2PWithoutCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (payment.HasCommission
                 && payment.OperationType == OperationType.Withdraw)
        {
            return await GetWithdrawWithCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
                 && payment.OperationType == OperationType.Withdraw)
        {
            return await GetWithdrawWithoutCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
                 && payment.OperationType == OperationType.Deposit)
        {
            return await GetDepositWithoutCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (payment.HasCommission
                 && payment.OperationType == OperationType.Deposit)
        {
            return await GetDepositWithCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
                 && payment.OperationType == OperationType.BillPayment)
        {
            return await GetBillPaymentWithoutCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (payment.HasCommission
                 && payment.OperationType == OperationType.BillPayment)
        {
            return await GetBillPaymentWithCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
                 && payment.OperationType == OperationType.BillPaymentCancellation)
        {
            return await GetBillPaymentCancelationWithoutCommissionPaymentLinesAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
                && payment.OperationType == OperationType.BuyPin)
        {
            return GetBuyPinWithoutCommissionPaymentLines(templateLines, payment);
        }
        else if (!payment.HasCommission && payment.OperationType == OperationType.PfPosBlockage)
        {
            var paymentLines = await GetPfPosBlockagePaymentLinesAsync(templateLines, payment);
            paymentLines.ForEach(x => x.LINEEXP = "Pos İşlem");
            return paymentLines;
        }
        else if (payment.OperationType == OperationType.PfBalance)
        {
            var paymentLines = await GetPfBalanceWithCommissionPaymentLinesAsync(templateLines, payment);
            paymentLines.ForEach(x => x.LINEEXP = "Üye İş Yeri Hakediş");
            return paymentLines;
        }
        else if (payment.OperationType == OperationType.PfCardTopupBalance)
        {
            var paymentLines = await GetPfCardTopupBalanceWithCommissionPaymentLinesAsync(templateLines, payment);
            paymentLines.ForEach(x => x.LINEEXP = "Üye İş Yeri Hakediş Card Topup");
            return paymentLines;
        }
        else if (payment.OperationType == OperationType.PfDeductionBalance)
        {
            var paymentLines = await GetPfDeductionBalancePaymentLinesAsync(templateLines, payment);
            paymentLines.ForEach(x => x.LINEEXP = "Üye İş Yeri Kesinti");
            return paymentLines;
        }

        else if (!payment.HasCommission && payment.OperationType == OperationType.CampaignPayment)
        {
            return GetCampaignWithdrawPaymentLines(templateLines, payment);
        }
        else if (!payment.HasCommission && payment.OperationType == OperationType.CampaignCashback)
        {
            return GetCampaignDepositPaymentLines(templateLines, payment);
        }
        else if (!payment.HasCommission && payment.OperationType == OperationType.ReturnCampaignPayment)
        {
            return GetCampaignDepositPaymentLines(templateLines, payment);
        }
        else if (!payment.HasCommission && payment.OperationType == OperationType.ReturnCampaignCashback)
        {
            return GetCampaignWithdrawPaymentLines(templateLines, payment);
        }
        else if (payment.HasCommission
            && payment.OperationType == OperationType.PaymentWithWallet)
        {
            return GetPaymentWithWalletLinesWithCommissionAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
               && payment.OperationType == OperationType.PaymentWithWallet)
        {
            return GetPaymentWithWalletLinesWithoutCommissionAsync(templateLines, payment);
        }
        else if (!payment.HasCommission
               && payment.OperationType == OperationType.Cashback)
        {
            return GetCashbackLinesWithoutCommissionAsync(templateLines, payment);
        }
        throw new InvalidParameterException(nameof(payment.OperationType));
    }

    private async Task<List<PaymentLine>> GetDepositWithCommissionPaymentLinesAsync(List<Template> templateLines, Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (item.TemplateExpenseType == TemplateExpenseType.BsmvAndCommission)
            {
                paymentLine.AMOUNT = payment.ReceiverCommissionAmount + payment.ReceiverBsmvAmount;
                paymentLine.CUSTOMER_ID = payment.Destination;
            }
            else if (item.TemplateExpenseType == TemplateExpenseType.Bsmv)
            {
                paymentLine.AMOUNT = payment.ReceiverBsmvAmount;
            }
            else if (item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.AMOUNT = payment.ReceiverCommissionAmount;
            }

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "A" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                     && item.Direction == "B"
                     && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment,item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }
            else if (item.TranCode == "SAT_FAT")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
                paymentLine.AMOUNT = payment.ReceiverCommissionAmount;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private List<PaymentLine> GetCampaignWithdrawPaymentLines(List<Template> templateLines,
    Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "B" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private List<PaymentLine> GetCampaignDepositPaymentLines(List<Template> templateLines,
    Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "A" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetPfBalanceWithCommissionPaymentLinesAsync(List<Template> templateLines,
   Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CUSTOMER_ID = payment.Source,
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (!string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.CUSTOMER_ID = string.Empty;
                paymentLine.ACCOUNTNO = IsAccountNumberStatic(item.AccountNumber) ? item.AccountNumber : await GetBankAccountNumberAsync(payment,item.AccountNumber);
            }

            paymentLine.AMOUNT = item.TemplateExpenseType switch
            {
                TemplateExpenseType.Commission => Math.Round(payment.CommissionAmount - payment.BsmvAmount,2),
                TemplateExpenseType.Bsmv => Math.Round(payment.BsmvAmount,2),
                TemplateExpenseType.BsmvAndCommission => Math.Round(payment.CommissionAmount,2),
                _ => Math.Round(paymentLine.AMOUNT,2)
            };

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }
    
    private async Task<List<PaymentLine>> GetPfDeductionBalancePaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CUSTOMER_ID = payment.Source,
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (!string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.CUSTOMER_ID = string.Empty;
                paymentLine.ACCOUNTNO = IsAccountNumberStatic(item.AccountNumber) ? item.AccountNumber : await GetBankAccountNumberAsync(payment,item.AccountNumber);
            }

            paymentLine.AMOUNT = item.TemplateExpenseType switch
            {
                TemplateExpenseType.Chargeback => Math.Round(payment.ChargebackAmount,2),
                TemplateExpenseType.Suspicious => Math.Round(payment.SuspiciousAmount,2),
                TemplateExpenseType.Due => Math.Round(payment.DueAmount,2),
                TemplateExpenseType.Deduction => Math.Round(payment.Amount,2),
                TemplateExpenseType.DeductionReturn => Math.Round(payment.ChargebackReturnAmount + payment.SuspiciousReturnAmount,2),
                TemplateExpenseType.ChargebackReturn => Math.Round(payment.ChargebackReturnAmount,2),
                TemplateExpenseType.SuspiciousReturn => Math.Round(payment.SuspiciousReturnAmount,2),
                _ => Math.Round(paymentLine.AMOUNT,2)
            };

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetPfCardTopupBalanceWithCommissionPaymentLinesAsync(List<Template> templateLines,
Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CUSTOMER_ID = payment.Source,
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                          && item.Direction == "A"
                                                          && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment, item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            if (!string.IsNullOrEmpty(item.AccountNumber) && IsAccountNumberStatic(item.AccountNumber))
            {
                paymentLine.ACCOUNTNO = item.AccountNumber;
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount - payment.BsmvAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Bsmv)
            {
                paymentLine.AMOUNT = payment.BsmvAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.BsmvAndCommission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount;
            }

            paymentLines.Add(paymentLine);
        }
        return paymentLines;
    }


    private async Task<List<PaymentLine>> GetWithdrawWithoutCommissionPaymentLinesAsync(List<Template> templateLines,
    Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "B" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                               && item.Direction == "A"
                                                               && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment, item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private List<PaymentLine> GetBuyPinWithoutCommissionPaymentLines(List<Template> templateLines, Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "B" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetBillPaymentCancelationWithoutCommissionPaymentLinesAsync(
        List<Template> templateLines, Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "A" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                               && item.Direction == "B"
                                                               && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment, item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetBillPaymentWithCommissionPaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber)
                && item.Direction == "B"
                && item.TranCode == "VRM"
                && item.TemplateExpenseType == TemplateExpenseType.Amount)
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                               && item.Direction == "A"
                                                               && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment, item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }
            else if (item.TranCode == "SAT_FAT" && item.TemplateExpenseType == TemplateExpenseType.BsmvAndCommission)
            {
                paymentLine.CUSTOMER_ID = payment.Source;
                paymentLine.AMOUNT = payment.CommissionAmount + payment.BsmvAmount;
            }
            else if (item.TranCode == "SAT_FAT" && item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.CUSTOMER_ID = payment.Source;
                paymentLine.AMOUNT = payment.CommissionAmount;
            }
            else if (item.Direction == "A" && item.TemplateExpenseType == TemplateExpenseType.Bsmv)
            {
                paymentLine.AMOUNT = payment.BsmvAmount;
            }
            else if (item.Direction == "A" && item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount;
            }
            else if (item.Direction == "B" && item.TranCode == "VRM" && string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.CUSTOMER_ID = payment.Source;
                paymentLine.AMOUNT = payment.CommissionAmount + payment.BsmvAmount;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetBillPaymentWithoutCommissionPaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "B" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                               && item.Direction == "A"
                                                               && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment, item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetDepositWithoutCommissionPaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber)
                && item.Direction == "A"
                && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                               && item.Direction == "B"
                                                               && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment,item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }
    
    private async Task<List<PaymentLine>> GetPfPosBlockagePaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            paymentLine.AMOUNT = item.TemplateExpenseType switch
            {
                TemplateExpenseType.AmountWithReturn => Math.Round(payment.Amount - payment.ReturnAmount,2),
                TemplateExpenseType.AmountWithoutBankCommission => Math.Round(payment.Amount - payment.ReturnAmount - payment.BankCommissionAmount,2),
                TemplateExpenseType.BankCommission => Math.Round(payment.BankCommissionAmount,2),
                TemplateExpenseType.Commission => Math.Round(payment.CommissionAmount - payment.BsmvAmount,2),
                TemplateExpenseType.Bsmv => Math.Round(payment.BsmvAmount,2),
                TemplateExpenseType.ReturnAmount => Math.Round(payment.ReturnAmount,2),
                TemplateExpenseType.BsmvAndCommission => Math.Round(payment.CommissionAmount,2),
                _ => Math.Round(paymentLine.AMOUNT,2)
            };

            if (string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
                paymentLine.ACCOUNTNO = string.Empty;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber))
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment,item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetWithdrawWithCommissionPaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (item.TemplateExpenseType == TemplateExpenseType.BsmvAndCommission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount + payment.BsmvAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Bsmv)
            {
                paymentLine.AMOUNT = payment.BsmvAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount;
            }

            if (string.IsNullOrEmpty(item.AccountNumber) && item.Direction == "B" && item.TranCode == "VRM")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (!string.IsNullOrEmpty(item.AccountNumber) && !IsAccountNumberStatic(item.AccountNumber)
                                                               && item.Direction == "A"
                                                               && item.TranCode == "VRM")
            {
                paymentLine.ACCOUNTNO = await GetBankAccountNumberAsync(payment, item.AccountNumber);
                paymentLine.CUSTOMER_ID = string.Empty;
            }
            else if (item.TranCode == "SAT_FAT" && item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.CUSTOMER_ID = payment.Source;
                paymentLine.AMOUNT = payment.CommissionAmount;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<string> GetBankAccountNumberAsync(Payment payment, string accountTag)
    {
        var bankAccount = await _bankAccount.GetAll()
            .Where(s => s.BankCode == payment.BankCode &&
                        s.AccountTag == accountTag &&
                        s.AccountingTransactionType == payment.AccountingTransactionType)
            .SingleOrDefaultAsync();

        if (bankAccount is null)
        {
            throw new NotFoundException($"BankCode - {payment.BankCode} AccountingTransactionType - {payment.AccountingTransactionType}");
        }

        return bankAccount.AccountNumber;
    }


    private async Task<List<Template>> GetTemplateLinesAsync(Payment payment)
    {
        var templateLines = _templateRepository.GetAll()
            .Where(s => s.OperationType == payment.OperationType
                        && s.HasCommission == payment.HasCommission
                        && s.RecordStatus == RecordStatus.Active);

        if (!templateLines.Any())
        {
            throw new NotFoundException(nameof(templateLines));
        }

        return await templateLines.ToListAsync();
    }

    private async Task<List<PaymentLine>> GetP2PWithoutCommissionPaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.ACCOUNTNO = await GetAccountNumberAsync(payment.CurrencyCode, payment.AccountingCustomerType);
            }

            if (item.Direction == "B")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (item.Direction == "A")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<List<PaymentLine>> GetP2PWithCommissionPaymentLinesAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                CUSTOMER_ID = payment.Source,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                ACCOUNTNO = item.AccountNumber,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (item.TemplateExpenseType == TemplateExpenseType.BsmvAndCommission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount + payment.BsmvAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Bsmv)
            {
                paymentLine.AMOUNT = payment.BsmvAmount;
            }

            if (string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.ACCOUNTNO = await GetAccountNumberAsync(payment.CurrencyCode, payment.AccountingCustomerType);
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Bsmv
                || item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.CUSTOMER_ID = string.Empty;
            }

            if (item.Direction == "A" && item.TemplateExpenseType == TemplateExpenseType.Amount)
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private List<PaymentLine> GetPaymentWithWalletLinesWithCommissionAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                CUSTOMER_ID = payment.Source,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };

            if (item.TemplateExpenseType == TemplateExpenseType.BsmvAndCommission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount + payment.BsmvAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Commission)
            {
                paymentLine.AMOUNT = payment.CommissionAmount;
            }

            if (item.TemplateExpenseType == TemplateExpenseType.Bsmv)
            {
                paymentLine.AMOUNT = payment.BsmvAmount;
            }

            if (item.Direction == "B" && item.TranCode == "VRM" && string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (item.Direction == "A" && item.TranCode == "VRM" && string.IsNullOrEmpty(item.AccountNumber))
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private List<PaymentLine> GetPaymentWithWalletLinesWithoutCommissionAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };


            if (item.Direction == "B")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (item.Direction == "A")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private List<PaymentLine> GetCashbackLinesWithoutCommissionAsync(List<Template> templateLines,
        Payment payment)
    {
        var paymentLines = new List<PaymentLine>();
        foreach (var item in templateLines)
        {
            var paymentLine = new PaymentLine
            {
                CURRENCYCODE = payment.CurrencyCode,
                DIRECTION = item.Direction,
                TRCODE = item.TranCode,
                AMOUNT = payment.Amount,
                SRV_CODE = item.SrvCode,
                GENEXP2 = payment.TransactionId
            };


            if (item.Direction == "B")
            {
                paymentLine.CUSTOMER_ID = payment.Source;
            }
            else if (item.Direction == "A")
            {
                paymentLine.CUSTOMER_ID = payment.Destination;
            }

            paymentLines.Add(paymentLine);
        }

        return paymentLines;
    }

    private async Task<string> GetAccountNumberAsync(string currencyCode, AccountingCustomerType accountingCustomerType)
    {
        var currency = await GetExternalCurrencyFromCacheAsync(currencyCode, accountingCustomerType);
        if (currency is null)
        {
            throw new NotFoundException(nameof(currency));
        }

        return currency.AccountCode;
    }

    private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        var correlationId = Guid.NewGuid();

        await RequestIntegrationLogAsync(value, correlationId);

        var logoSettings = _vaultClient.GetSecretValue<LogoSettings>("AccountingSecrets", "LogoSettings");

        //AddCredential
        requestUri = $"{requestUri}?user={logoSettings.User}&pass={HttpUtility.UrlEncode(logoSettings.Password)}&firmNo={logoSettings.FirmNo}";

        var jsonValue = JsonContent.Create(value);
        var jsonString = await jsonValue.ReadAsStringAsync();

        byte[] contentBytes = Encoding.UTF8.GetBytes(jsonString);
        var byteArrayContent = new ByteArrayContent(contentBytes);
        byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _client.PostAsync(requestUri, byteArrayContent);


        if (!response.IsSuccessStatusCode)
        {
            await ResponseIntegrationLogAsync(correlationId, response);
            var exceptionDetails = await response.Content.ReadAsStringAsync();
            throw new Exception(exceptionDetails);
        }

        await ResponseIntegrationLogAsync(correlationId, response);

        return response;
    }

    private async Task ResponseIntegrationLogAsync(Guid correlationId, HttpResponseMessage response)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
                ("IntegrationLoggerState", "Tiger");
            if (isLogEnable.ParameterValue != "True")
            {
                return;
            }
            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = "Tiger",
                Type = nameof(IntegrationLogType.Accounting),
                Date = DateTime.Now,
                Response = await response.Content.ReadAsStringAsync(),
                HttpCode = response.StatusCode.ToString(),
                DataType = IntegrationLogDataType.Json,
                ErrorCode = !response.IsSuccessStatusCode ? response.StatusCode.ToString() : "",
                ErrorMessage = !response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : string.Empty,
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
            await endpoint.Send(log, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: Accounting - Exception {exception}");
        }
    }

    private async Task RequestIntegrationLogAsync<T>(T value, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
                ("IntegrationLoggerState", "Tiger");
            if (isLogEnable.ParameterValue != "True")
            {
                return;
            }

            var jsonValue = JsonContent.Create(value);
            var jsonString = await jsonValue.ReadAsStringAsync();

            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = "Tiger",
                Type = nameof(IntegrationLogType.Accounting),
                Date = DateTime.Now,
                Request = jsonString,
                DataType = IntegrationLogDataType.Json
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
            await endpoint.Send(log, cancellationToken.Token);

        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: Accounting - Exception {exception}");
        }
    }

    private async Task<HttpResponseMessage> DeletePaymentAsync(string requestUri, string referenceId, int externalOperationType)
    {
        var logoSettings = _vaultClient.GetSecretValue<LogoSettings>("AccountingSecrets", "LogoSettings");

        //AddCredential
        requestUri = $"{requestUri}?user={logoSettings.User}&pass={logoSettings.Password}&firmNo={logoSettings.FirmNo}&REFERANCE={referenceId}&OPR_TYPE={externalOperationType}";

        var response = await _client.DeleteAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"{requestUri} is not responding.Response StatusCode: {response.StatusCode}");
        }

        return response;
    }

    private async Task<ExternalCurrency> GetExternalCurrencyFromCacheAsync(string currencyCode, AccountingCustomerType accountingCustomerType)
    {
        var currency = await _cacheService.GetOrCreateAsync($"{currencyCode}_{accountingCustomerType}",
            async () => await GetExternalCurrencyAsync(currencyCode, accountingCustomerType));

        return currency;
    }

    private async Task<ExternalCurrency> GetExternalCurrencyAsync(string currencyCode, AccountingCustomerType accountingCustomerType)
    {
        try
        {
            var currency = await _currencyRepository.GetAll()
                .Where(s => s.Code == currencyCode && s.AccountingCustomerType == accountingCustomerType).SingleOrDefaultAsync();

            if (currency is null)
            {
                throw new NotFoundException(nameof(currency));
            }

            return currency;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ErrorOnGetExternalCurrency detail: \n{ex}");
            throw;
        }
    }

    private async Task<string> GenerateReferenceIdAsync()
    {
        bool any;
        string reference;

        var random = new Random();
        do
        {
            reference = $"REF{_randomGenerator.GenerateSecureRandomNumber(10).ToString(CultureInfo.InvariantCulture)}";

            any = await _paymentRepository.GetAll()
                .AnyAsync(s => s.ReferenceId == reference);
        } while (any);

        return reference;
    }

    public Task<Guid> ProcessInvoiceAsync(ProcessInvoiceRequest processInvoiceRequest)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAccountingCustomerAsync(UpdateAccountingCustomer updateAccountingCustomer)
    {
        var customers = await _customerRepository.GetAll().Where(c => c.CustomerCode.Equals(updateAccountingCustomer.OldCustomerCode)).ToListAsync();

        if (customers.Any())
        {
            foreach (var customer in customers)
            {
                customer.CustomerCode = updateAccountingCustomer.CustomerCode ?? customer.CustomerCode;
                customer.FirstName = updateAccountingCustomer.FirstName ?? customer.FirstName;
                customer.LastName = updateAccountingCustomer.LastName ?? customer.LastName;
                customer.Email = updateAccountingCustomer.Email ?? customer.Email;
                customer.PhoneNumber = updateAccountingCustomer.PhoneNumber ?? customer.PhoneNumber;
                customer.PhoneCode = updateAccountingCustomer.PhoneCode ?? customer.PhoneCode;
                customer.IdentityNumber = updateAccountingCustomer.IdentityNumber ?? customer.IdentityNumber;
                customer.Title = updateAccountingCustomer.Title ?? customer.Title;
                customer.City = updateAccountingCustomer.City ?? customer.City;
                customer.CityCode = updateAccountingCustomer.CityCode ?? customer.CityCode;
                customer.Country = updateAccountingCustomer.Country ?? customer.Country;
                customer.CountryCode = updateAccountingCustomer.CountryCode ?? customer.CountryCode;
                customer.Address = updateAccountingCustomer.Address ?? customer.Address;
                customer.TaxNumber = updateAccountingCustomer.TaxNumber ?? customer.TaxNumber;
                customer.TaxOffice = updateAccountingCustomer.TaxOffice ?? customer.TaxOffice;
                customer.TaxOfficeCode = updateAccountingCustomer.TaxOfficeCode ?? customer.TaxOfficeCode;
                customer.Town = updateAccountingCustomer.Town ?? customer.Town;
                customer.District = updateAccountingCustomer.District ?? customer.District;

                try
                {
                    var customerResponse = await PostCustomerAsync(customer);

                    customer.IsSuccess = customerResponse.isSuccess;
                    customer.ResultMessage = customerResponse.strMesage.Length > MaxResultMessageSize
                                            ? customerResponse.strMesage.Substring(0, MaxResultMessageSize)
                                            : customerResponse.strMesage;

                    await _customerRepository.UpdateAsync(customer);
                    await SaveCustomerAuditLogAsync(customer);
                }
                catch (Exception exception)
                {
                    customer.IsSuccess = false;
                    customer.ResultMessage = exception.Message.Length > MaxResultMessageSize
                                            ? exception.Message.Substring(0, MaxResultMessageSize)
                                            : exception.Message;

                    await _customerRepository.UpdateAsync(customer);
                    await SaveCustomerAuditLogAsync(customer);

                    _logger.LogError($"ErrorRetryPostCustomer Error: {exception}");
                }
            }
        }
    }

    private bool IsAccountNumberStatic(string accountNumber) =>
        !(accountNumber.StartsWith("{{") && accountNumber.EndsWith("}}"));
}