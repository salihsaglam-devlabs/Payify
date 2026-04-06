using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests;
using LinkPara.Accounting.Application.Commons.Models.DefaultCustomerConfiguration;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using LinkPara.HttpProviders.BusinessParameter;
using MassTransit;
using System.Text.Json;
using ServiceRequestEnums = LinkPara.Accounting.Application.Commons.Models.AlternatifAccountingRequests.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.Accounting.Application.Commons.Exceptions;
using LinkPara.Cache;
using LinkPara.Accounting.Infrastructure.Persistence;
using LinkPara.Accounting.Application.Commons.Models;
using LinkPara.Security;

namespace LinkPara.Accounting.Infrastructure.Services.AccountingServices;

public class AlternatifAccountingService : IAccountingService
{
    private readonly HttpClient _client;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ILogger<AlternatifAccountingService> _logger;
    private readonly IVaultClient _vaultClient;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IBus _bus;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<BankAccount> _bankAccountRepository;
    private readonly ICacheService _cacheService;
    private readonly AccountingDbContext _dbContext;
    private readonly ISecureRandomGenerator _randomGenerator;

    private const int MaxResultMessageSize = 800;

    public AlternatifAccountingService(IGenericRepository<Customer> customerRepository,
        ILogger<AlternatifAccountingService> logger,
        HttpClient client,
        IVaultClient vaultClient,
        IGenericRepository<Payment> paymentRepository,
        IParameterService parameterService,
        IBus bus,
        IGenericRepository<BankAccount> bankAccount,
        ICacheService cacheService,
        AccountingDbContext dbContext,
        ISecureRandomGenerator randomGenerator)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _client = client;
        _vaultClient = vaultClient;
        _paymentRepository = paymentRepository;
        _parameterService = parameterService;
        _bus = bus;
        _bankAccountRepository = bankAccount;
        _cacheService = cacheService;
        _dbContext = dbContext;
        _randomGenerator = randomGenerator;
    }

    public Task CancelPaymentAsync(Guid clientReferenceId)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> ProcessInvoiceAsync(ProcessInvoiceRequest processInvoiceRequest)
    {
        try
        {
            if (!processInvoiceRequest.Payments.Any())
            {
                return Guid.Empty;
            }

            return await PostRegisterInvoicePaymentsAsync(processInvoiceRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError("Error On EmoneyRegisterInvoiceAsync exception : {exception}", exception);
        }
        return Guid.Empty;

    }

    private async Task<Guid> PostRegisterInvoicePaymentsAsync(ProcessInvoiceRequest processInvoiceRequest)
    {
        try
        {
            var invoiceId = SequentialGuid.NewSequentialGuid();
            var registerInvoiceServiceRequest = await PrepareRegisterInvoiceServiceRequestAsync(processInvoiceRequest, invoiceId);
            await PostRegisterInvoiceAsync(registerInvoiceServiceRequest);
            var registerInvoice = PrepareRegisterInvoice(registerInvoiceServiceRequest, processInvoiceRequest.TransactionType, invoiceId);
            await _dbContext.RegisterInvoice.AddAsync(registerInvoice);
            await _dbContext.SaveChangesAsync();
            return registerInvoice.Id;
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception On PostRegisterInvoice {exception}", exception);
        }
        return Guid.Empty;
    }

    private async Task<RegisterInvoiceRequest> PrepareRegisterInvoiceServiceRequestAsync(ProcessInvoiceRequest processInvoiceRequest, Guid invoiceId)
    {

        var customer = await _customerRepository.GetAll().Where(x => x.Code == processInvoiceRequest.CustomerCode).FirstOrDefaultAsync();

        if (customer is null)
        {
            throw new NotFoundException(nameof(customer), processInvoiceRequest.CustomerCode);
        }
        var customerRequest = MapCustomerToServiceRequest(customer);

        var registerInvoiceServiceRequest = new RegisterInvoiceRequest
        {
            musteri = customerRequest,
            bsmvToplamTutar = processInvoiceRequest.CommissionType == CommissionType.Sender
                                                                ? processInvoiceRequest.Payments.Sum(x => x.BsmvAmount)
                                                                : processInvoiceRequest.Payments.Sum(x => x.ReceiverBsmvAmount),
            islemKomisyonTutari = processInvoiceRequest.CommissionType == CommissionType.Sender
                                                                ? processInvoiceRequest.Payments.Sum(x => x.CommissionAmount)
                                                                : processInvoiceRequest.Payments.Sum(x => x.ReceiverCommissionAmount),
            aciklama = $"{customerRequest.musteriKodu} nolu müşteri fatura kaydı.",
            islemDovizKuru = "1",
            islemNo = invoiceId.ToString(),
            islemDovizTuru = "TRY",
            islemTarihi = DateTime.Now,
            islemSaati = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}",
        };

        if (processInvoiceRequest.Payments.Any())
        {
            var commissionDetailRequests = new List<CommissionDetailRequest>();

            processInvoiceRequest.Payments.ForEach((customerPayment) =>
                {
                    commissionDetailRequests.Add(new CommissionDetailRequest
                    {
                        dovizTuru = customerPayment.CurrencyCode,
                        islemKodu = "Komisyon",
                        islemAciklama = $"\"{invoiceId.ToString()}\" lu işlemin komisyon bedeli",
                        tutar = processInvoiceRequest.CommissionType == CommissionType.Sender
                                                ? customerPayment.CommissionAmount
                                                : customerPayment.ReceiverCommissionAmount,
                    });
                });
            registerInvoiceServiceRequest.komisyonDetaylari = commissionDetailRequests;
        }

        return registerInvoiceServiceRequest;
    }

    private async Task<RegisterInvoiceRequest> PrepareRegisterInvoiceServiceRequestAsync(Payment payment)
    {

        var customer = await _customerRepository.GetAll().Where(x => x.Code == payment.Source).FirstOrDefaultAsync();

        if (customer is null)
        {
            throw new NotFoundException(nameof(customer), payment.Source);
        }
        var customerRequest = MapCustomerToServiceRequest(customer);

        var registerInvoiceServiceRequest = new RegisterInvoiceRequest
        {
            musteri = customerRequest,
            bsmvToplamTutar = payment.BsmvAmount,
            islemKomisyonTutari = payment.Amount,
            aciklama = $"{customerRequest.musteriKodu} nolu müşteri fatura kaydı.",
            islemDovizKuru = "1",
            islemNo = string.Empty,
            islemDovizTuru = "TRY",
            islemSaati = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}",
            islemTarihi = DateTime.Now,
            komisyonDetaylari = new List<CommissionDetailRequest>()
        };

        return registerInvoiceServiceRequest;
    }

    private static Invoice PrepareRegisterInvoice(RegisterInvoiceRequest registerInvoiceServiceRequest, AccountingTransactionType transactionType, Guid invoiceId)
    {
        return new Invoice
        {
            Id = invoiceId,
            Code = registerInvoiceServiceRequest.musteri.musteriKodu,
            TotalBsmv = registerInvoiceServiceRequest.bsmvToplamTutar,
            TotalCommission = registerInvoiceServiceRequest.islemKomisyonTutari,
            TransactionDate = DateTime.Now,
            TransactionType = transactionType
        };

    }

    private async Task PostRegisterInvoiceAsync(RegisterInvoiceRequest registerInvoiceRequest)
    {
        try
        {
            await PostAsJsonAsync<RegisterInvoiceRequest>("api/Payment/FaturaKayit", registerInvoiceRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception on RegisterInvoiceAsync exception : {exception}", exception);
            throw;
        }

    }

    public async Task CreateCustomerAsync(AccountingCustomer customerRequest)
    {
        try
        {

            FillEmptyFieldsWithDefaultInformation(customerRequest);

            var customer = new Customer
            {
                Code = customerRequest.Code,
                CustomerCode = customerRequest.CustomerCode,
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
                AccountingCustomerCode = customerRequest.CustomerCode
            };

            await _customerRepository.AddAsync(customer);
        }
        catch (Exception exception)
        {
            _logger.LogError("Error Save Customer Exception: {exception}", exception);
            throw;
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

    public async Task PostPaymentAsync(AccountingPayment paymentRequest)
    {
        if (!string.IsNullOrEmpty(paymentRequest.ReferenceId))
        {
            await RetryPaymentAsync(paymentRequest);
            return;
        }

        Payment payment = await PreparePaymentAsync(paymentRequest);
        await SendPaymentAsync(payment);
        await _paymentRepository.AddAsync(payment);
    }

    private async Task SendPaymentAsync(Payment payment)
    {
        switch (payment.OperationType)
        {
            case OperationType.EmoneyTransfer:
                await PostEmoneyTransferAsync(payment);
                break;
            case OperationType.Withdraw:
                await PostWithdrawAsync(payment);
                break;
            case OperationType.Deposit:
                await PostDepositAsync(payment);
                break;
            case OperationType.PfPosBlockage:
                await PostPosBlockageAsync(payment);
                break;
            case OperationType.PfPosUnBlockage:
                await PostPosUnblockageAsync(payment);
                break;
            case OperationType.PfBalance:
                await PostPfBalanceAsync(payment);
                break;
            case OperationType.PfCustomerInvoice:
                await PostPfCustomerInvoiceAsync(payment);
                break;
            default:
                payment.IsSuccess = false;
                payment.ResultMessage = "ThereIsNoMethodThisType";
                break;
        }
    }

    private async Task PostPfCustomerInvoiceAsync(Payment payment)
    {
        try
        {
            var serviceRequest = await PrepareRegisterInvoiceServiceRequestAsync(payment);
            await PostAsJsonAsync<RegisterInvoiceRequest>("api/Payment/FaturaKayit", serviceRequest);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }
    }

    private async Task PostPfBalanceAsync(Payment payment)
    {
        try
        {
            var serviceRequest = await PreparePfBalanceServiceRequestAsync(payment);
            await PostAsJsonAsync<BankTransferRequest>("api/Payment/BankaHavaleKayit", serviceRequest);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }
    }

    private async Task<BankTransferRequest> PreparePfBalanceServiceRequestAsync(Payment payment)
    {
        CustomerRequest senderCustomer = await GetCustomerAsync(payment.Source);
        var bankAccountNumber = await GetBankAccountNumberFromCacheAsync(payment.BankCode, payment.AccountingTransactionType);

        return new BankTransferRequest
        {
            musteri = senderCustomer,
            islemNo = payment.ReferenceId,
            islemTarihi = payment.TransactionDate,
            masrafTutari = payment.CommissionAmount + payment.BsmvAmount,
            tutar = payment.Amount,
            bankaIslemTipi = ServiceRequestEnums.BankTransferType.GonderilenHavale.ToString(),
            dovizKuru = 1,
            dovizTuru = payment.CurrencyCode,
            aciklama = $"{senderCustomer.musteriKodu}-{payment.OperationType}-{payment.ReferenceId}",
            bankaHesapNo = bankAccountNumber.AccountNumber,
            bankaHesapAdi = bankAccountNumber.AccountName,
        };
    }

    private async Task RetryPaymentAsync(AccountingPayment paymentRequest)
    {
        var dbPayment = await _paymentRepository.GetAll()
            .FirstOrDefaultAsync(x => x.ReferenceId == paymentRequest.ReferenceId);        

        if (dbPayment is null)
        {
            throw new NotFoundException(nameof(dbPayment));
        }
        dbPayment.FailedPaymentRetryCount++;

        if (dbPayment.IsSuccess)
        {
            throw new CanNotRetrySuccessPaymentException();
        }

        try
        {
            await SendPaymentAsync(dbPayment);

            await _paymentRepository.UpdateAsync(dbPayment);
        }
        catch (Exception exception)
        {
            dbPayment.IsSuccess = false;
            dbPayment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;

            await _paymentRepository.UpdateAsync(dbPayment);

            _logger.LogError("ErrorRetrySavePayment Error: {exception}", exception);
        }
        if (!dbPayment.IsSuccess)
        {
            throw new RetryFailedException();
        }
    }

    private async Task PostEmoneyTransferAsync(Payment payment)
    {
        try
        {
            var paymentServiceRequest = await PrepareEmoneyTransferServiceRequestAsync(payment);
            await PostAsJsonAsync<P2PMoneyTransferRequest>("api/Payment/CuzdanTransfer", paymentServiceRequest);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }
    }

    private async Task PostWithdrawAsync(Payment payment)
    {
        try
        {
            var withdrawServiceRequest = await PrepareWithdrawServiceRequestAsync(payment);
            await PostAsJsonAsync<BankTransferRequest>("api/Payment/BankaHavaleKayit", withdrawServiceRequest);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }
    }

    private async Task PostDepositAsync(Payment payment)
    {
        try
        {
            var depositServiceRequest = await PrepareDepositServiceRequestAsync(payment);
            await PostAsJsonAsync<BankTransferRequest>("api/Payment/BankaHavaleKayit", depositServiceRequest);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }
    }

    private async Task PostPosBlockageAsync(Payment payment)
    {
        try
        {
            var request = await PreparePosBlockageRequestAsync(payment);
            await PostAsJsonAsync<PosBlockageRequest>("api/Payment/SanalPosKayit", request);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }

    }

    private async Task<PosBlockageRequest> PreparePosBlockageRequestAsync(Payment payment)
    {
        CustomerRequest senderCustomer = await GetCustomerAsync(payment.Destination);
        var bankAccountNumber = await GetBankAccountNumberFromCacheAsync(payment.BankCode, payment.AccountingTransactionType);

        return new PosBlockageRequest
        {
            sanalPosIslemTipi = ServiceRequestEnums.PostBlockageType.TopluIslem,
            aciklama = $"{DateTime.Now.ToString("dd.MM.yyyy")} tarihli işlemler.",
            islemBankaHesapNo = bankAccountNumber.AccountNumber,
            islemBankaKodu = bankAccountNumber.AccountName,
            islemSaati = DateTime.Now.ToString("HH:mm"),
            islemTaksitSayisi = 0,
            islemTarihi = DateTime.Now,
            islemTutari = payment.Amount,
            musteri = senderCustomer
        };
    }

    private async Task PostPosUnblockageAsync(Payment payment)
    {
        try
        {
            var request = await PreparePosUnblockageRequestAsync(payment);
            await PostAsJsonAsync<PosUnblockageRequest>("api/Payment/POSBlokeCozme", request);
            payment.IsSuccess = true;
        }
        catch (Exception exception)
        {
            payment.IsSuccess = false;
            payment.ResultMessage = exception.Message.Length > MaxResultMessageSize
                ? exception.Message.Substring(0, MaxResultMessageSize)
                : exception.Message;
            _logger.LogError("PostEmoneyTransfer Error Exception {exception}", exception);
        }

    }

    private async Task<PosUnblockageRequest> PreparePosUnblockageRequestAsync(Payment payment)
    {
        var bankAccountNumber = await GetBankAccountNumberFromCacheAsync(payment.BankCode, payment.AccountingTransactionType);

        return new PosUnblockageRequest
        {
            dovizTuru = payment.CurrencyCode,
            dovizKuru = 1,
            aciklama = "UnBlockage",
            bankaHesapAdi = bankAccountNumber.AccountName,
            bankaHesapNo = bankAccountNumber.AccountNumber,
            islemTarihi = DateTime.Now,
            posHesapAdi = $"{payment.Destination} hesabı",
            posHesapNo = payment.IbanNumber,
            tutar = payment.Amount,
            islemNo = string.Empty
        };
    }

    private async Task<BankTransferRequest> PrepareWithdrawServiceRequestAsync(Payment payment)
    {
        CustomerRequest senderCustomer = await GetCustomerAsync(payment.Source);
        var bankAccountNumber = await GetBankAccountNumberFromCacheAsync(payment.BankCode, payment.AccountingTransactionType);

        return new BankTransferRequest
        {
            musteri = senderCustomer,
            islemNo = payment.ReferenceId,
            islemTarihi = payment.TransactionDate,
            masrafTutari = payment.CommissionAmount + payment.BsmvAmount,
            tutar = payment.Amount,
            bankaIslemTipi = ServiceRequestEnums.BankTransferType.GonderilenHavale.ToString(),
            dovizKuru = 1,
            dovizTuru = payment.CurrencyCode,
            aciklama = $"{senderCustomer.unvan} a ait {payment.Source} dan {ServiceRequestEnums.BankTransferType.GonderilenHavale.ToString()}",
            bankaHesapNo = bankAccountNumber.AccountNumber,
            bankaHesapAdi = bankAccountNumber.AccountName,
        };
    }

    private async Task<BankAccount> GetBankAccountNumberFromCacheAsync(int bankCode, AccountingTransactionType accountingTransactionType)
    {
        var accountNumber = await _cacheService.GetOrCreateAsync($"{bankCode}_{accountingTransactionType}",
            async () => await GetBankAccountNumberAsync(bankCode, accountingTransactionType));

        return accountNumber;
    }

    private async Task<BankAccount> GetBankAccountNumberAsync(int bankCode, AccountingTransactionType accountingTransactionType)
    {
        try
        {
            var bankAccount = await _bankAccountRepository.GetAll().Where(x => 
                x.BankCode == bankCode && 
                x.AccountingTransactionType == accountingTransactionType &&
                x.AccountTag == "{{BankAccountNumber}}"
                ).FirstOrDefaultAsync();

            if (bankAccount is null)
            {
                throw new NotFoundException(nameof(bankAccount), bankCode);
            }
            return bankAccount;
        }
        catch (Exception exception)
        {
            _logger.LogError("Error On GetBankAccountAsync Exception:{exception}", exception);
            throw;
        }
    }

    private async Task<BankTransferRequest> PrepareDepositServiceRequestAsync(Payment payment)
    {
        CustomerRequest senderCustomer = await GetCustomerAsync(payment.Destination);
        var bankAccountNumber = await GetBankAccountNumberFromCacheAsync(payment.BankCode, payment.AccountingTransactionType);

        return new BankTransferRequest
        {
            musteri = senderCustomer,
            islemNo = payment.ReferenceId,
            islemTarihi = payment.TransactionDate,
            masrafTutari = payment.ReceiverCommissionAmount + payment.ReceiverBsmvAmount,
            tutar = payment.Amount,
            bankaIslemTipi = ServiceRequestEnums.BankTransferType.GelenHavale.ToString(),
            dovizKuru = 1,
            dovizTuru = payment.CurrencyCode,
            aciklama = $"{senderCustomer.unvan} a ait {payment.Destination} a {ServiceRequestEnums.BankTransferType.GelenHavale.ToString()}",
            bankaHesapNo = bankAccountNumber.AccountNumber,
            bankaHesapAdi = bankAccountNumber.AccountName,

        };
    }

    private async Task<Payment> PreparePaymentAsync(AccountingPayment paymentRequest)
    {
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
            IbanNumber = paymentRequest.IbanNumber,
        };

        return payment;
    }

    private async Task<string> GenerateReferenceIdAsync()
    {
        bool any;
        string reference;
        do
        {
            reference = $"REF{_randomGenerator.GenerateSecureRandomNumber(10).ToString(CultureInfo.InvariantCulture)}";

            any = await _paymentRepository.GetAll()
                .AnyAsync(s => s.ReferenceId == reference);
        } while (any);

        return reference;
    }

    private async Task<P2PMoneyTransferRequest> PrepareEmoneyTransferServiceRequestAsync(Payment payment)
    {
        CustomerRequest senderCustomer = await GetCustomerAsync(payment.Source);
        CustomerRequest receiverCustomer = await GetCustomerAsync(payment.Destination);

        string commissionType = CalculateCommissionType(payment);

        return new P2PMoneyTransferRequest
        {
            musteriAlici = receiverCustomer,
            musteri = senderCustomer,
            islemNo = payment.ReferenceId,
            islemTarihi = payment.TransactionDate,
            komisyonGonderenTutari = payment.CommissionAmount + payment.BsmvAmount,
            komisyonAliciTutari = payment.ReceiverCommissionAmount + payment.ReceiverBsmvAmount,
            komisyonOdemeSekli = commissionType,
            tutar = payment.Amount,
            dovizKuru = 1,
            dovizTuru = payment.CurrencyCode,
            aciklama = $"{senderCustomer.unvan} a ait {payment.Source} dan {receiverCustomer.unvan} a ait {payment.Destination} a Transfer"
        };

    }

    private static string CalculateCommissionType(Payment payment)
    {
        if (payment.CommissionAmount > 0 && payment.ReceiverCommissionAmount > 0)
        {
            return ServiceRequestEnums.CommissionType.KomisyonParcaliOdeme.ToString();
        }
        else if (payment.CommissionAmount > 0)
        {
            return ServiceRequestEnums.CommissionType.KomisyonuGonderenOder.ToString();
        }
        else if (payment.ReceiverCommissionAmount > 0)
        {
            return ServiceRequestEnums.CommissionType.KomisyonuAliciOder.ToString();
        }
        return ServiceRequestEnums.CommissionType.KomisyonuGonderenOder.ToString();
    }

    private async Task<CustomerRequest> GetCustomerAsync(string customerCode)
    {
        var customer = await _customerRepository.GetAll()
            .Where(x => x.Code == customerCode)
            .FirstOrDefaultAsync();

        if (customer is null)
        {
            throw new NotFoundException(nameof(Customer), customerCode);
        }

        return MapCustomerToServiceRequest(customer);
    }

    private CustomerRequest MapCustomerToServiceRequest(Customer customer)
    {
        return new CustomerRequest
        {
            adi = customer.FirstName ?? string.Empty,
            soyadi = customer.LastName ?? string.Empty,
            unvan = customer.Title ?? string.Empty,
            adres = customer.Address ?? string.Empty,
            email = customer.Email ?? string.Empty,
            ilce = customer.Town ?? string.Empty,
            musteriKodu = customer.CustomerCode ?? string.Empty,
            sehir = customer.City ?? string.Empty,
            sehirKodu = customer.CityCode ?? string.Empty,
            tckNo = customer.IdentityNumber ?? string.Empty,
            telefon1 = customer.PhoneNumber ?? string.Empty,
            ulke = customer.Country ?? string.Empty,
            ulkeKodu = customer.CountryCode ?? string.Empty,
            vergiDairesi = customer.TaxOffice ?? string.Empty,
            vergiNo = customer.TaxNumber ?? string.Empty,
            ilceKodu = customer.CityCode ?? string.Empty,
            fax1 = string.Empty,
            fax2 = string.Empty,
            fax3 = string.Empty,
            ozelKod1 = string.Empty,
            ozelKod2 = string.Empty,
            ozelKod3 = string.Empty,
            ozelKod4 = string.Empty,
            postaKodu = string.Empty,
            telefon2 = string.Empty,
            telefon3 = string.Empty,
        };
    }

    private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        var correlationId = Guid.NewGuid();

        await RequestIntegrationLogAsync(value, correlationId);
        var response = await _client.PostAsJsonAsync(requestUri, value);

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
                ("IntegrationLoggerState", "Alternatif");
            if (isLogEnable.ParameterValue != "True")
            {
                return;
            }
            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = "Alternatif",
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
            _logger.LogError("SendToRawDataQueue Error: Accounting - Exception {exception}", exception);
        }
    }

    private async Task RequestIntegrationLogAsync<T>(T value, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
                ("IntegrationLoggerState", "Alternatif");

            if (isLogEnable.ParameterValue != "True")
            {
                return;
            }
            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = "Alternatif",
                Type = nameof(IntegrationLogType.Accounting),
                Date = DateTime.Now,
                Request = JsonSerializer.Serialize(value),
                DataType = IntegrationLogDataType.Json
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
            await endpoint.Send(log, cancellationToken.Token);

        }
        catch (Exception exception)
        {
            _logger.LogError("SendToRawDataQueue Error: Accounting - Exception {exception}", exception);
        }
    }

    public async Task UpdateAccountingCustomerAsync(UpdateAccountingCustomer updateAccountingCustomer)
    {
        var customers = await _customerRepository.GetAll().Where(c => c.CustomerCode.Equals(updateAccountingCustomer.OldCustomerCode)).ToListAsync();

        if (customers.Any())
        {
            customers.ForEach(c =>
            {
                c.CustomerCode = updateAccountingCustomer.CustomerCode ?? c.CustomerCode;
                c.FirstName = updateAccountingCustomer.FirstName ?? c.FirstName;
                c.LastName = updateAccountingCustomer.LastName ?? c.LastName;
                c.Email = updateAccountingCustomer.Email ?? c.Email;
                c.PhoneNumber = updateAccountingCustomer.PhoneNumber ?? c.PhoneNumber;
                c.PhoneCode = updateAccountingCustomer.PhoneCode ?? c.PhoneCode;
                c.IdentityNumber = updateAccountingCustomer.IdentityNumber ?? c.IdentityNumber;
                c.Title = updateAccountingCustomer.Title ?? c.Title;
                c.City = updateAccountingCustomer.City ?? c.City;
                c.CityCode = updateAccountingCustomer.CityCode ?? c.CityCode;
                c.Country = updateAccountingCustomer.Country ?? c.Country;
                c.CountryCode = updateAccountingCustomer.CountryCode ?? c.CountryCode;
                c.Address = updateAccountingCustomer.Address ?? c.Address;
                c.TaxNumber = updateAccountingCustomer.TaxNumber ?? c.TaxNumber;
                c.TaxOffice = updateAccountingCustomer.TaxOffice ?? c.TaxOffice;
                c.TaxOfficeCode = updateAccountingCustomer.TaxOfficeCode ?? c.TaxOfficeCode;
                c.Town = updateAccountingCustomer.Town ?? c.Town;
                c.District = updateAccountingCustomer.District ?? c.District;
            });

            await _customerRepository.UpdateRangeAsync(customers);
        }
    }

}
