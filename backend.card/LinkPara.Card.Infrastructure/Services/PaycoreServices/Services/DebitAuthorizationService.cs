using System.Globalization;
using AutoMapper;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.DebitAuthorizationModels;
using LinkPara.Card.Application.Features.PaycoreServices.DebitAuthorizationServices.Commands.DebitAuthorization;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Services
{
    public class DebitAuthorizationService : IPaycoreDebitAuthorizationService
    {
        private readonly IConfiguration _configuration;
        private readonly IVaultClient _vaultClient;
        private readonly PaycoreSettings _paycoreSettings;
        private readonly CardDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DebitAuthorizationService> _logger;
        private readonly IGenericRepository<DebitAuthorization> _debitAuthRepository;
        private readonly IGenericRepository<DebitAuthorizationFee> _debitAuthFeeRepository;
        private readonly IGenericRepository<CustomerWalletCard> _walletCardRepository;
        private readonly IWalletService _walletService;
        private readonly ICustomerTransactionService _transactionService;
        private readonly IContextProvider _contextProvider;

        public const string BalanceInfoType = "CURRENT";
        public const string Delimiter = "_";
        public const string DateTimeFormat = "yyyyMMddHHmmss";
        public const string PaycoreUser = "Paycore";

        public DebitAuthorizationService(IConfiguration configuration,
          IVaultClient vaultClient,
          CardDbContext dbContext,
          IContextProvider contextProvider,
          IMapper mapper,
          ILogger<DebitAuthorizationService> logger,
          IGenericRepository<DebitAuthorization> debitAuthorizationRepository,
          IGenericRepository<DebitAuthorizationFee> debitAuthorizationFeeRepository,
          IGenericRepository<CustomerWalletCard> walletCardRepository,
          IWalletService walletService,
          ICustomerTransactionService transactionService)
        {
            _configuration = configuration;
            _vaultClient = vaultClient;
            _paycoreSettings = new PaycoreSettings();
            _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
            _paycoreSettings.VaultSettings = _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _debitAuthRepository = debitAuthorizationRepository;
            _debitAuthFeeRepository = debitAuthorizationFeeRepository;
            _walletService = walletService;
            _contextProvider = contextProvider;
            _walletCardRepository = walletCardRepository;
            _transactionService = transactionService;
        }

        public async Task<DebitAuthorizationResponse> DebitAuthAsync(DebitAuthorizationCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            DebitAuthorizationResponse response = new DebitAuthorizationResponse();
            String paycoreResponseCode = "";
            String paycoreResponseMsg = "";

            try
            {

                bool existingRecord = _dbContext.DebitAuthorization.Any(x => x.CorrelationID == command.CorrelationID &&
                          x.RequestType == TxnRequestTypes.NORMAL);

                if (existingRecord)
                {
                    _logger.LogError("Debit authorization dublicated transaction : {command.CorrelationID}", command.CorrelationID);
                    return GenerateDebitAuthResponse(command, null, TransactionResponseCodes.McrDuplicatedTransaction, EmoneyResponseCodes.McrDuplicatedTransaction);
                }

                DebitAuthorization debitAuthorization = await CreateDebitAuthorizationRecord(command);

                if (command.TransactionType != null && !CheckTxnType(command.TransactionType))
                {
                    _logger.LogError("Debit authorization unknown transaction type : {command.TransactionType}", command.TransactionType);
                    return GenerateDebitAuthResponse(command, null, TransactionResponseCodes.DisallowedCardTransaction, EmoneyResponseCodes.DisallowedCardTransaction7Msg);
                }

                if (TxnTypes.INQUIRYBALANCE.Equals(command.TransactionType) && !command.FeeList.Any())
                {
                    _logger.LogError("Balance inquiry must contain fee list : {command.CorrelationID}", command.CorrelationID);
                    paycoreResponseMsg = "Validation Error! Balance inquiry must contain fee list";
                    return GenerateDebitAuthResponse(command, null, TransactionResponseCodes.SystemError96, paycoreResponseMsg);
                }

                if (!debitAuthorization.IsReturn)
                {
                    UpdateBalanceResponse txnResponse = await CallUpdateBalanceService(command);
                    response = GenerateDebitAuthResponse(command, txnResponse, txnResponse.ResponseCode, null);
                }
                else
                {
                    response = GenerateDebitAuthResponse(command, null, TransactionResponseCodes.Approved, EmoneyResponseCodes.ApprovedMsg);
                }
            }
            catch (NotFoundException nex)
            {
                paycoreResponseCode = TransactionResponseCodes.OriginalNotFound;
                paycoreResponseMsg = EmoneyResponseCodes.OriginalNotFoundMsg;
                _logger.LogError(paycoreResponseCode, nex);
            }
            catch (Exception ex)
            {
                if (command.RequestType == TxnRequestTypes.VOID || command.RequestType == TxnRequestTypes.REVERSAL)
                    paycoreResponseCode = TransactionResponseCodes.Approved;
                else
                {
                    paycoreResponseCode = TransactionResponseCodes.SystemError96;
                    paycoreResponseMsg = ex.Message;
                }
                _logger.LogError(paycoreResponseCode, ex);
            }
            finally
            {
                if (!paycoreResponseCode.Equals(""))
                    response = GenerateDebitAuthResponse(command, null, paycoreResponseCode, paycoreResponseMsg);
            }

            return response;
        }

        private DebitAuthorizationResponse GenerateDebitAuthResponse(DebitAuthorizationCommand command, UpdateBalanceResponse txnResponse, String responseCode, String responseMsg)
        {
            DebitAuthorizationResponse response = new DebitAuthorizationResponse();
            response.CorrelationID = command.CorrelationID;

            if (txnResponse != null && EmoneyResponseCodes.Approved.Equals(txnResponse.ResponseCode))
            {
                response.BillingAmount = new PaycoreAmount();
                response.BillingAmount.Amount = command.BillingAmount.Amount;
                response.BillingAmount.CurrencyCode = command.BillingAmount.CurrencyCode;
                response.TransactionAmount = new PaycoreAmount();
                response.TransactionAmount.Amount = command.TransactionAmount.Amount;
                response.TransactionAmount.CurrencyCode = command.TransactionAmount.CurrencyCode;

                BalanceInfo balanceInfo = new BalanceInfo();
                balanceInfo.Type = BalanceInfoType;
                balanceInfo.CurrencyCode = command.BillingAmount.CurrencyCode;
                balanceInfo.CurrentAmount = txnResponse.CurrentBalance;
                balanceInfo.PreviousAmount = 0;
                balanceInfo.TransactionAmount = (long)command.BillingAmount.Amount;

                response.BalanceInformationList = new List<BalanceInfo> { balanceInfo };
                response.BankingRefNo = txnResponse.TransactionId.ToString();
                DebitAuthorizationTxnResponse debitAuthorizationTxnResponse = MapPaycoreResponseCodes(txnResponse.ResponseCode);
                response.ResponseCode = debitAuthorizationTxnResponse.ResponseCode;
                response.ResponseMessage = debitAuthorizationTxnResponse.ResponseMessage;
                response.ResponseDescription = debitAuthorizationTxnResponse.ResponseDescription;
                response.IsApproved = (response.ResponseCode == TransactionResponseCodes.Approved);
            }
            else if (command.TransactionType == TxnTypes.REFERENCEDREFUND || command.TransactionType == TxnTypes.REFUND) {
                response.BillingAmount = new PaycoreAmount();
                response.BillingAmount.Amount = command.BillingAmount.Amount;
                response.BillingAmount.CurrencyCode = command.BillingAmount.CurrencyCode;
                response.TransactionAmount = new PaycoreAmount();
                response.TransactionAmount.Amount = command.TransactionAmount.Amount;
                response.TransactionAmount.CurrencyCode = command.TransactionAmount.CurrencyCode;
                BalanceInfo balanceInfo = new BalanceInfo();
                balanceInfo.Type = BalanceInfoType;
                balanceInfo.CurrencyCode = command.BillingAmount.CurrencyCode;
                balanceInfo.CurrentAmount = 0;
                balanceInfo.PreviousAmount = 0;
                balanceInfo.TransactionAmount = (long)command.BillingAmount.Amount;
                response.ResponseCode = TransactionResponseCodes.Approved;
                response.ResponseMessage = EmoneyResponseCodes.Approved;
                response.ResponseDescription = EmoneyResponseCodes.Approved;
                response.IsApproved = true;
            }
            else
            {
                if(txnResponse != null && txnResponse.ResponseCode != null)
                {
                    responseCode = txnResponse.ResponseCode;
                }

                response.ResponseCode = responseCode;
                response.ResponseMessage = responseMsg ?? EmoneyResponseCodes.SystemError96;
                response.ResponseDescription = responseMsg ?? EmoneyResponseCodes.SystemError96Msg;
                response.IsApproved = (response.ResponseCode == TransactionResponseCodes.Approved);
            }
            return response;
        }

        private async Task<UpdateBalanceResponse> CallUpdateBalanceService(DebitAuthorizationCommand command)
        {
            try
            {
                UpdateBalanceRequest updateBalanceRequest = new UpdateBalanceRequest();
                updateBalanceRequest.Utid = command.OceanTxnGUID.ToString();
                updateBalanceRequest.TransactionType = MapPaycoreTxnTypes(command.TransactionType);

                if (command.TransactionType == TxnTypes.ATMCASHIN || command.TransactionType == TxnTypes.KKPT)
                    updateBalanceRequest.TransactionDirection = EmoneyTransactionDirection.MoneyIn;
                else
                    updateBalanceRequest.TransactionDirection = EmoneyTransactionDirection.MoneyOut;

                updateBalanceRequest.Channel = command.TerminalType + Delimiter + command.EntryType;
                updateBalanceRequest.CurrencyCode = command.BillingAmount.CurrencyCode.ToString();
                decimal parsedAmount = command.BillingAmount.Amount / (decimal)Math.Pow(10, 2);
                updateBalanceRequest.Amount = parsedAmount;

                decimal feeAmount = (decimal)(command.FeeList?
                    .Sum(x => x.Amount));

                updateBalanceRequest.FeeAmount = feeAmount;
                updateBalanceRequest.CommissionAmount = 0;
                updateBalanceRequest.Description = command.MerchantName;
                updateBalanceRequest.IsBalanceControl = true;

                string dateTimeString = command.RequestDate.ToString() + command.RequestTime.ToString("D6");
                DateTime requestDateTime = DateTime.ParseExact(
                    dateTimeString,
                    DateTimeFormat,
                    CultureInfo.InvariantCulture
                );

                updateBalanceRequest.TransactionDate = requestDateTime;

                var walletNumber = await _walletCardRepository.GetAll().FirstOrDefaultAsync(x => x.CardNumber == command.CardNo).Select(x => x.WalletNumber);
                updateBalanceRequest.WalletNumber = walletNumber;

                if (walletNumber == null)
                {
                    return new UpdateBalanceResponse
                    {
                        ResponseCode = TransactionResponseCodes.InvalidAccountNo
                    };
                }

                var validationResponse = await _walletService.ValidateWalletAsync(new ValidateWalletRequest
                {
                    WalletNumber = walletNumber.ToString(),
                    CurrencyCode = command.TransactionAmount.CurrencyCode.ToString()
                });

                if(validationResponse != null && !EmoneyResponseCodes.Approved.Equals(validationResponse.ResponseCode))
                {
                    return new UpdateBalanceResponse { ResponseCode = validationResponse.ResponseCode};
                } 

                if (command.RequestType == TxnRequestTypes.VOID || command.RequestType == TxnRequestTypes.REVERSAL)
                {
                    CustomerTransactionRequest customerTransactionRequest = new CustomerTransactionRequest();
                    customerTransactionRequest.CustomerTransactionId = command.OceanTxnGUID.ToString();
                    CustomerTransactionResponse transactionResponse = await _transactionService.GetTransactionsByCustomerTransactionIdAsync(customerTransactionRequest);

                    if (!transactionResponse.CustomerTransactions.Any())
                    {
                        throw new NotFoundException();
                    }

                    updateBalanceRequest.Utid = transactionResponse.CustomerTransactions.FirstOrDefault().CustomerTransactionId;

                    EmoneyTransactionDirection txnDirection = updateBalanceRequest.TransactionDirection;
                    if (txnDirection == EmoneyTransactionDirection.MoneyIn)
                        updateBalanceRequest.TransactionDirection = EmoneyTransactionDirection.MoneyOut;
                    else
                        updateBalanceRequest.TransactionDirection = EmoneyTransactionDirection.MoneyIn;
                }

                if (command.TransactionType == TxnTypes.KKPT)
                {
                    string transferInformation = $"{Delimiter}{command.TransferInformation.Type}{Delimiter}{command.TransferInformation.Name}{Delimiter}{command.TransferInformation.CardNo}{Delimiter}{command.TransferInformation.BusinesssApplicationIdentifier}";
                    updateBalanceRequest.Description.Concat(transferInformation);
                }

                return await _walletService.UpdateBalanceAsync(updateBalanceRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError("Update Balance Service error: {ex}", ex);
                throw;
            }
        }

        private EmoneyTransactionType MapPaycoreTxnTypes(String paycoreTxnType)
        {
            switch (paycoreTxnType)
            {
                case TxnTypes.SALE:
                case TxnTypes.INSURANCE:
                case TxnTypes.BILLPAYMENT:
                    return EmoneyTransactionType.Sale;
                case TxnTypes.REFUND:
                case TxnTypes.REFERENCEDREFUND:
                    return EmoneyTransactionType.Return;
                case TxnTypes.ATMCASHIN:
                case TxnTypes.KKPT:
                    return EmoneyTransactionType.Deposit;
                case TxnTypes.WITHDRAWAL:
                    return EmoneyTransactionType.Withdraw;
                case TxnTypes.INQUIRYBALANCE:
                    return EmoneyTransactionType.Maintenance;
                default:
                    throw new GenericException("Transaction Type Not Match", paycoreTxnType);
            }
        }

        private DebitAuthorizationTxnResponse MapPaycoreResponseCodes(String emoneyResponseCode)
        {
            DebitAuthorizationTxnResponse response = new DebitAuthorizationTxnResponse();

            switch (emoneyResponseCode)
            {
                case EmoneyResponseCodes.Approved:
                    response.ResponseCode = TransactionResponseCodes.Approved;
                    response.ResponseMessage = EmoneyResponseCodes.Approved;
                    response.ResponseDescription = EmoneyResponseCodes.ApprovedMsg;
                    break;
                case EmoneyResponseCodes.NotApproved:
                    response.ResponseCode = TransactionResponseCodes.NotApproved;
                    response.ResponseMessage = EmoneyResponseCodes.NotApproved;
                    response.ResponseDescription = EmoneyResponseCodes.NotApprovedMsg;
                    break;
                case EmoneyResponseCodes.NotApproved2:
                    response.ResponseCode = TransactionResponseCodes.NotApproved;
                    response.ResponseMessage = EmoneyResponseCodes.NotApproved2;
                    response.ResponseDescription = EmoneyResponseCodes.NotApproved2Msg;
                    break;
                case EmoneyResponseCodes.NotApproved3:
                    response.ResponseCode = TransactionResponseCodes.NotApproved;
                    response.ResponseMessage = EmoneyResponseCodes.NotApproved3;
                    response.ResponseDescription = EmoneyResponseCodes.NotApproved3Msg;
                    break;
                case EmoneyResponseCodes.NotApproved4:
                    response.ResponseCode = TransactionResponseCodes.NotApproved;
                    response.ResponseMessage = EmoneyResponseCodes.NotApproved4;
                    response.ResponseDescription = EmoneyResponseCodes.NotApproved4Msg;
                    break;
                case EmoneyResponseCodes.NotApproved5:
                    response.ResponseCode = TransactionResponseCodes.NotApproved;
                    response.ResponseMessage = EmoneyResponseCodes.NotApproved5;
                    response.ResponseDescription = EmoneyResponseCodes.NotApproved5Msg;
                    break;
                case EmoneyResponseCodes.InvalidOperation:
                    response.ResponseCode = TransactionResponseCodes.InvalidOperation;
                    response.ResponseMessage = EmoneyResponseCodes.InvalidOperation;
                    response.ResponseDescription = EmoneyResponseCodes.InvalidOperationMsg;
                    break;
                case EmoneyResponseCodes.InvalidOperation2:
                    response.ResponseCode = TransactionResponseCodes.InvalidOperation;
                    response.ResponseMessage = EmoneyResponseCodes.InvalidOperation2;
                    response.ResponseDescription = EmoneyResponseCodes.InvalidOperation2Msg;
                    break;
                case EmoneyResponseCodes.InvalidAmount:
                    response.ResponseCode = TransactionResponseCodes.InvalidAmount;
                    response.ResponseMessage = EmoneyResponseCodes.InvalidAmount;
                    response.ResponseDescription = EmoneyResponseCodes.InvalidAmountMsg;
                    break;
                case EmoneyResponseCodes.InvalidAmount2:
                    response.ResponseCode = TransactionResponseCodes.InvalidAmount;
                    response.ResponseMessage = EmoneyResponseCodes.InvalidAmount2;
                    response.ResponseDescription = EmoneyResponseCodes.InvalidAmount2Msg;
                    break;
                case EmoneyResponseCodes.InvalidAmount3:
                    response.ResponseCode = TransactionResponseCodes.InvalidAmount;
                    response.ResponseMessage = EmoneyResponseCodes.InvalidAmount3;
                    response.ResponseDescription = EmoneyResponseCodes.InvalidAmount3Msg;
                    break;
                case EmoneyResponseCodes.InvalidAccountNo:
                    response.ResponseCode = TransactionResponseCodes.InvalidAccountNo;
                    response.ResponseMessage = EmoneyResponseCodes.InvalidAccountNo;
                    response.ResponseDescription = EmoneyResponseCodes.InvalidAccountNoMsg;
                    break;
                case EmoneyResponseCodes.InsufficientLimit:
                    response.ResponseCode = TransactionResponseCodes.InsufficientLimit;
                    response.ResponseMessage = EmoneyResponseCodes.InsufficientLimit;
                    response.ResponseDescription = EmoneyResponseCodes.InsufficientLimitMsg;
                    break;
                case EmoneyResponseCodes.InsufficientLimit2:
                    response.ResponseCode = TransactionResponseCodes.InsufficientLimit;
                    response.ResponseMessage = EmoneyResponseCodes.InsufficientLimit2;
                    response.ResponseDescription = EmoneyResponseCodes.InsufficientLimit2Msg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransactionMsg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction2:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction2;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransaction2Msg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction3:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction3;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransaction3Msg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction4:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction4;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransaction4Msg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction5:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction5;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransaction5Msg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction6:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction6;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransaction6Msg;
                    break;
                case EmoneyResponseCodes.DisallowedCardTransaction7:
                    response.ResponseCode = TransactionResponseCodes.DisallowedCardTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.DisallowedCardTransaction7;
                    response.ResponseDescription = EmoneyResponseCodes.DisallowedCardTransaction7Msg;
                    break;
                case EmoneyResponseCodes.ExceededCashLimit:
                    response.ResponseCode = TransactionResponseCodes.ExceededCashLimit;
                    response.ResponseMessage = EmoneyResponseCodes.ExceededCashLimit;
                    response.ResponseDescription = EmoneyResponseCodes.ExceededCashLimitMsg;
                    break;
                case EmoneyResponseCodes.ExceedPinTryLimit:
                    response.ResponseCode = TransactionResponseCodes.ExceedPinTryLimit;
                    response.ResponseMessage = EmoneyResponseCodes.ExceedPinTryLimit;
                    response.ResponseDescription = EmoneyResponseCodes.ExceedPinTryLimitMsg;
                    break;
                case EmoneyResponseCodes.McrDuplicatedTransaction:
                    response.ResponseCode = TransactionResponseCodes.McrDuplicatedTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.McrDuplicatedTransaction;
                    response.ResponseDescription = EmoneyResponseCodes.McrDuplicatedTransactionMsg;
                    break;
                case EmoneyResponseCodes.McrDuplicatedTransaction2:
                    response.ResponseCode = TransactionResponseCodes.McrDuplicatedTransaction;
                    response.ResponseMessage = EmoneyResponseCodes.McrDuplicatedTransaction2;
                    response.ResponseDescription = EmoneyResponseCodes.McrDuplicatedTransaction2Msg;
                    break;
                case EmoneyResponseCodes.SystemError96:
                    response.ResponseCode = TransactionResponseCodes.SystemError96;
                    response.ResponseMessage = EmoneyResponseCodes.SystemError96;
                    response.ResponseDescription = EmoneyResponseCodes.SystemError96Msg;
                    break;
                case EmoneyResponseCodes.SystemError96_2:
                    response.ResponseCode = TransactionResponseCodes.SystemError96;
                    response.ResponseMessage = EmoneyResponseCodes.SystemError96_2;
                    response.ResponseDescription = EmoneyResponseCodes.SystemError96_2Msg;
                    break;
                case EmoneyResponseCodes.SystemError96_3:
                    response.ResponseCode = TransactionResponseCodes.SystemError96;
                    response.ResponseMessage = EmoneyResponseCodes.SystemError96_3;
                    response.ResponseDescription = EmoneyResponseCodes.SystemError96_3Msg;
                    break;
                case EmoneyResponseCodes.SystemError96_4:
                    response.ResponseCode = TransactionResponseCodes.SystemError96;
                    response.ResponseMessage = EmoneyResponseCodes.SystemError96_4;
                    response.ResponseDescription = EmoneyResponseCodes.SystemError96_4Msg;
                    break;
                default:
                    response.ResponseCode = TransactionResponseCodes.SystemError96;
                    response.ResponseMessage = EmoneyResponseCodes.SystemError96;
                    response.ResponseDescription = EmoneyResponseCodes.SystemError96Msg;
                    break;
            }

            return response;
        }

        private async Task<DebitAuthorization> CreateDebitAuthorizationRecord(DebitAuthorizationCommand command)
        {
            try
            {
                DebitAuthorization debitAuthorization = new DebitAuthorization();
                debitAuthorization.AccountBranch = command.AccountBranch;
                debitAuthorization.AccountCurrency = command.AccountCurrency;
                debitAuthorization.AccountNo = command.AccountNo;
                debitAuthorization.AccountSuffix = command.AccountSuffix;
                debitAuthorization.AcquirerCountryCode = command.AcquirerCountryCode;
                debitAuthorization.AcquirerId = command.AcquirerId;
                debitAuthorization.BankingCustomerNo = command.BankingCustomerNo;
                debitAuthorization.BankingRefNo = command.BankingRefNo;
                debitAuthorization.BillingAmount = command.BillingAmount.Amount;
                debitAuthorization.BillingCurrency = command.BillingAmount.CurrencyCode;
                debitAuthorization.CardNo = command.CardNo;
                debitAuthorization.CardDci = command.CardDci;
                debitAuthorization.CardBrand = command.CardBrand;
                debitAuthorization.Channel = command.Channel;
                debitAuthorization.CorrelationID = command.CorrelationID;
                debitAuthorization.EntryType = command.EntryType;
                debitAuthorization.ExpirationTime = command.ExpirationTime;
                debitAuthorization.Iban = command.Iban;
                debitAuthorization.IsAdvice = command.IsAdvice;
                debitAuthorization.IsSimulation = command.IsSimulation;
                debitAuthorization.Mcc = command.Mcc;
                debitAuthorization.MerchantId = command.MerchantId;
                debitAuthorization.MerchantName = command.MerchantName;
                debitAuthorization.NationalSwitchId = command.NationalSwitchId;
                debitAuthorization.OceanTxnGUID = command.OceanTxnGUID;
                debitAuthorization.ProvisionCode = command.ProvisionCode;
                debitAuthorization.RequestDate = command.RequestDate;
                debitAuthorization.RequestTime = command.RequestTime;
                debitAuthorization.RequestType = command.RequestType;
                debitAuthorization.QRData = command.QRData;
                debitAuthorization.Rrn = command.Rrn;
                debitAuthorization.SecurityLevelIndicator = command.SecurityLevelIndicator;
                debitAuthorization.TerminalId = command.TerminalId;
                debitAuthorization.TransactionAmount = command.TransactionAmount.Amount;
                debitAuthorization.TransactionCurrency = command.TransactionAmount.CurrencyCode;
                debitAuthorization.TransactionType = command.TransactionType;
                debitAuthorization.TerminalType = command.TerminalType;
                debitAuthorization.TransactionSource = command.TransactionSource;
                debitAuthorization.CreatedBy = _contextProvider.CurrentContext.UserId ?? PaycoreUser;
                debitAuthorization.CreateDate = DateTime.UtcNow;

                if (command.TransferInformation != null)
                    debitAuthorization.BusinesssApplicationIdentifier = command.TransferInformation.BusinesssApplicationIdentifier;

                if (command.TransactionActionFlags != null)
                    debitAuthorization.PartialAcceptor = command.TransactionActionFlags.PartialAcceptor;

                if (command.ReplacementBillingAmount != null)
                {
                    debitAuthorization.ReplacementBillingAmount = command.ReplacementBillingAmount.Amount;
                    debitAuthorization.ReplacementBillingCurrency = command.ReplacementBillingAmount.CurrencyCode;
                }

                if (command.ReplacementTransactionAmount != null)
                {
                    debitAuthorization.ReplacementTransactionAmount = command.ReplacementTransactionAmount.Amount;
                    debitAuthorization.ReplacementTransactionCurrency = command.ReplacementTransactionAmount.CurrencyCode;
                }

                if (command.TransferInformation != null)
                {
                    debitAuthorization.TransferInformationType = command.TransferInformation.Type;
                    debitAuthorization.TransferInformationName = command.TransferInformation.Name;
                    debitAuthorization.TransferInformationCardNo = command.TransferInformation.CardNo;
                }

                if (command.TransactionType == TxnTypes.REFERENCEDREFUND || command.TransactionType == TxnTypes.REFUND)
                {
                    debitAuthorization.IsReturn = true;
                }
                else
                {
                    debitAuthorization.IsReturn = false;
                }

                await _debitAuthRepository.AddAsync(debitAuthorization);

                foreach(var fee in command.FeeList)
                {

                    await _debitAuthFeeRepository.AddAsync(new DebitAuthorizationFee
                    {
                        Type = fee.Type,
                        Amount = fee.Amount,
                        CreatedBy = _contextProvider.CurrentContext.UserId ?? PaycoreUser,
                        CreateDate = DateTime.UtcNow,
                        CurrencyCode = fee.CurrencyCode,
                        OceanTxnGUID = command.OceanTxnGUID,
                        Tax1Amount = fee.Tax1Amount,
                        Tax2Amount = fee.Tax2Amount
                    });
                }
                return debitAuthorization;
            }
            catch (Exception ex)
            {
                _logger.LogError("Create Debit Authorization Record error: {ex}", ex);
                throw;
            }
        }

        private bool CheckTxnType(String txnType)
        {
            List<String> txnTypes = TxnTypes.ALL;

            if (txnTypes.Contains(txnType))
            {
                return true;
            }
            return false;
        }
    }
}
