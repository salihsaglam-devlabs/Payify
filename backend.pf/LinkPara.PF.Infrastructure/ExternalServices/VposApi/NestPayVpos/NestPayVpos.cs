using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using Microsoft.Extensions.Logging;
using LinkPara.HttpProviders.BusinessParameter;
using MassTransit;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Response;
using System.Globalization;
using System.Text;
using LinkPara.SharedModels.Banking.Enums;
using System;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos;

public class NestPayVpos : VposBase, IVposApi
{
    private readonly string ThreeDFullSecureStatus = "Y";
    private readonly string ThreeDHalfSecureStatus = "A";
    private static readonly string[] MdStatusCodes = { "1", "2", "3", "4" };
    private const string SuccessCode = "00";
    private const string StoreType = "3D";
    private const string HashAlgorithm = "ver3";
    private const string PaymentDetailOrderStatus = "QUERY";
    private const string QueryName = "MAXIPUANSORGU";

    private const string Ziraat = "ZiraatBankVpos";
    private const string Akbank = "AkBankVpos";
    private const string Halkbank = "HalkBankVpos";
    private const string Seker = "SekerBankVpos";
    private const string IsBank = "IsBankVpos";
    private const string Tfkb = "TurkiyeFinansKatilimBankVpos";
    private const string AnadoluBank = "AnadoluBankVpos";
    private const string Nestpay = "NestPayVpos";

    private NestPayPosInfo _nestPayPos;
    private readonly ILogger<NestPayVpos> _logger;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;

    public NestPayVpos(ILogger<NestPayVpos> logger, IParameterService parameterService, IBus bus)
    {
        _logger = logger;
        _parameterService = parameterService;
        _bus = bus;
    }
    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        NestPayInit3dModelRequest commonRequest = new NestPayInit3dModelRequest
        {
            ClientId = _nestPayPos.ClientId.Trim(),
            Pan = request.CardNumber,
            ExpireMonth = request.ExpireMonth.ToString(),
            ExpireYear = (request.ExpireYear.Length == 4) ? request.ExpireYear.Substring(2, 2) : request.ExpireYear,
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrderNumber,
            LanguageCode = request.LanguageCode != "undefined" ? request.LanguageCode.ToLower() : "tr",
            StoreType = StoreType,
            StoreKey = _nestPayPos.StoreKey,
            IsBlockaged = request.IsBlockaged,
            HashAlgorithm = HashAlgorithm,
            NumberOfInstallments = request.Installment,
            SubMerchantId = CleanUnicodeCharacters(request.SubMerchantCode),
            SubMerchantName = CleanUnicodeCharacters(request.SubMerchantName),
            SubMerchantCity = CleanUnicodeCharacters(request.SubMerchantCity),
            SubMerchantCountry = request.SubMerchantCountry,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantTaxNumber = request.SubMerchantTaxNumber,
            Type = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth",
            OkUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            FailUrl = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}",
            ClientIp = request.ClientIp,
            BlockageCode = request.BlockageCode,
            PostUrl = _nestPayPos.ThreeDSecureUrl,
            Cvv = request.Cvv2
        };

        string threedRequest;
        string bankName = "";
        if (_nestPayPos.BankCode == (int)BankCode.Akbank)
        {
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            commonRequest.VisaPfId = _nestPayPos.VisaPfId.Trim();
            threedRequest = commonRequest
                .BuildRequestAkbank();

            bankName = Akbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Ziraat)
        {
            commonRequest.SubMerchantId = request.ServiceProviderPspMerchantId;
            threedRequest = commonRequest.BuildRequestZiraat();

            bankName = Ziraat;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Halkbank)
        {
            commonRequest.SubMerchantId = request.SubMerchantId;
            threedRequest = commonRequest.BuildRequestHalkbank();

            bankName = Halkbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.SekerBank)
        {
            commonRequest.SubMerchantNumber = request.SubMerchantCode;
            threedRequest = commonRequest.BuildRequestSekerbank();

            bankName = Seker;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.IsBank)
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            threedRequest = commonRequest
                .BuildRequest();

            bankName = IsBank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.TurkiyeFinansKatilim)
        {
            commonRequest.SubMerchantTaxNumber = request.SubMerchantTaxNumber;
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            threedRequest = commonRequest.BuildRequestTfkb();

            bankName = Tfkb;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.AnadoluBank)
        {
            threedRequest = commonRequest.BuildRequestAnadoluBank();

            bankName = AnadoluBank;
        }
        else
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            threedRequest = commonRequest
                .BuildRequest();

            bankName = Nestpay;
        }

        await SendIntegrationRequest(threedRequest, Guid.NewGuid(), IntegrationLogDataType.Html, bankName);

        return new PosInit3DModelResponse
        {
            IsSuccess = true,
            HtmlContent = Base64Encode(threedRequest),
            ResponseCode = string.Empty,
            ResponseMessage = string.Empty,
        };
    }
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
    private string CleanUnicodeCharacters(string input)
    {
        var unaccentedMessage = String.Join("", input.Normalize(NormalizationForm.FormD)
             .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        return unaccentedMessage.Replace("ı", "i");
    }
    private string GetHash(Dictionary<string, string> form)
    {
        List<KeyValuePair<String, String>> postParams = new List<KeyValuePair<String, String>>();
        foreach (string key in form.Keys)
        {
            if (key != "encoding" && key != "HASH")
            {
                KeyValuePair<String, String> newKeyValuePair = new KeyValuePair<String, String>(key, form[key]);
                postParams.Add(newKeyValuePair);

            }
        }

        postParams.Sort(
            delegate (KeyValuePair<string, string> firstPair,
            KeyValuePair<string, string> nextPair)
            {
                return firstPair.Key.ToLower(new System.Globalization.CultureInfo("en-US", false)).CompareTo(nextPair.Key.ToLower(new System.Globalization.CultureInfo("en-US", false)));
            }
        );

        String hashVal = "";
        String storeKey = _nestPayPos.StoreKey;

        foreach (KeyValuePair<String, String> pair in postParams)
        {
            String escapedValue = pair.Value.Replace("\\", "\\\\").Replace("|", "\\|");
            String lowerValue = pair.Key.ToLower(new System.Globalization.CultureInfo("en-US", false));
            if (!"encoding".Equals(lowerValue) && !"hash".Equals(lowerValue))
            {
                hashVal += escapedValue + "|";
            }
        }
        hashVal += storeKey;

        var actualHash = VposHelper.GetSha1512(hashVal);

        return actualHash;
    }
    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        var commonRequest = new NestPayment3dModelRequest
        {
            MerchantName = _nestPayPos.MerchantName.Trim(),
            Password = _nestPayPos.Password.Trim(),
            ClientId = _nestPayPos.ClientId.Trim(),
            Pan = request.MD,
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrderNumber,
            NumberOfInstallments = request.Installment ?? 1,
            PayerAuthenticationCode = request.Cavv,
            PayerSecurityLevel = request.Eci,
            IsBlockaged = request.IsBlockaged,
            PayerTxnId = request.PayerTxnId,
            SubMerchantId = CleanUnicodeCharacters(request.SubMerchantCode),
            SubMerchantName = CleanUnicodeCharacters(request.SubMerchantName),
            SubMerchantCity = CleanUnicodeCharacters(request.SubMerchantCity),
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantCountry = request.SubMerchantCountry,
            SubMerchantTaxNumber = request.SubMerchantTaxNumber,
            Type = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth",
            BonusAmount = request.BonusAmount,
            BlockageCode = request.BlockageCode,
            ClientIp = request.ClientIp
        };

        string authRequest;
        string bankName = "";
        if (_nestPayPos.BankCode == (int)BankCode.Akbank)
        {
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            commonRequest.VisaPfId = _nestPayPos.VisaPfId.Trim();
            authRequest = commonRequest
                .BuildRequestAkbank();

            bankName = Akbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Ziraat)
        {
            commonRequest.SubMerchantId = request.ServiceProviderPspMerchantId;
            authRequest = commonRequest.BuildRequestZiraat();

            bankName = Ziraat;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Halkbank)
        {
            commonRequest.SubMerchantId = request.SubMerchantId;
            authRequest = commonRequest.BuildRequestHalkbank();

            bankName = Halkbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.SekerBank)
        {
            commonRequest.SubMerchantNumber = request.SubMerchantCode;
            authRequest = commonRequest.BuildRequestSekerbank();

            bankName = Seker;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.IsBank)
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            authRequest = commonRequest
                .BuildRequest();

            bankName = IsBank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.TurkiyeFinansKatilim)
        {
            commonRequest.SubMerchantTaxNumber = request.SubMerchantTaxNumber;
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            authRequest = commonRequest.BuildRequestTfkb();

            bankName = Tfkb;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.AnadoluBank)
        {
            authRequest = commonRequest.BuildRequestAnadoluBank();

            bankName = AnadoluBank;
        }
        else
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            authRequest = commonRequest
                .BuildRequest();

            bankName = Nestpay;
        }

        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, authRequest, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPayment3dModelResponse()
        .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ErrorCode;
            response.ResponseMessage = $"{parseResponse.ErrorMessage} - {parseResponse.ErrorDetailMessage}";
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var paymentDetailRequest = new NestPaymentDetailRequest
        {
            MerchantName = _nestPayPos.MerchantName,
            Password = _nestPayPos.Password,
            ClientId = _nestPayPos.ClientId,
            OrderId = request.OrderNumber,
            OrderStatus = PaymentDetailOrderStatus
        }.BuildRequest();

        string bankName = "";
        switch (_nestPayPos.BankCode)
        {
            case (int)BankCode.Akbank:
                bankName = Akbank;
                break;
            case (int)BankCode.Ziraat: 
                bankName = Ziraat;
                break;
            case (int)BankCode.Halkbank:
                bankName = Halkbank;
                break;
            case (int)BankCode.SekerBank:
                bankName = Seker;
                break;
            case (int)BankCode.IsBank:
                bankName = IsBank;
                break;
            case (int)BankCode.TurkiyeFinansKatilim:
                bankName = Tfkb;
                break;
            case (int)BankCode.AnadoluBank:
                bankName = AnadoluBank;
                break;
            default:
                bankName = Nestpay;
                break;
        }

        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, paymentDetailRequest, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPaymentDetailResponse()
           .Parse(content);

        var response = new PosPaymentDetailResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.OrderId;
            response.TransactionDate = parseResponse.TrxDate;
            response.Amount = FormatResponseAmount(parseResponse.Amount);
            response.TrxDate = parseResponse.TrxDate;
            response.AuthCode = parseResponse.AuthCode;
            response.OrderStatus = GetOrderStatus(parseResponse);

        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.OrderStatus = OrderStatus.Unknown;
        }

        return response;
    }
    private decimal FormatResponseAmount(string amount)
    {
        return !string.IsNullOrEmpty(amount) ? decimal.Parse(amount, CultureInfo.InvariantCulture) : 0;
    }
    private static OrderStatus GetOrderStatus(NestPaymentDetailResponse response)
    {
        if (response.ChargeType is "C")
        {
            return OrderStatus.Refunded;
        }

        return response.TransactionStatus switch
        {
            "D" => OrderStatus.Rejected,
            "V" => OrderStatus.Cancelled,
            "A" => OrderStatus.PreAuth,
            "C" => response.TrxDate.Date == DateTime.Now.Date ? OrderStatus.WaitingEndOfDay : OrderStatus.EndOfDayCompleted,
            _ => OrderStatus.Unknown
        };
    }

    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        var commonRequest = new NestPayNonSecureRequest
        {
            MerchantName = _nestPayPos.MerchantName.Trim(),
            Password = _nestPayPos.Password.Trim(),
            ClientId = _nestPayPos.ClientId.Trim(),
            Pan = request.CardNumber,
            Expiry = expiryDate,
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrderNumber,
            NumberOfInstallments = request.Installment ?? 1,
            IsBlockaged = request.IsBlockaged,
            Cvv = request.Cvv2,
            BonusAmount = request.BonusAmount ?? 0,
            SubMerchantId = CleanUnicodeCharacters(request.SubMerchantCode),
            SubMerchantName = CleanUnicodeCharacters(request.SubMerchantName),
            SubMerchantCity = CleanUnicodeCharacters(request.SubMerchantCity),
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantCountry = request.SubMerchantCountry,
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantTaxNumber = request.SubMerchantTaxNumber,
            Type = request.AuthType == VposAuthType.PreAuth ? "PreAuth" : "Auth",
            BlockageCode = request.BlockageCode,
            ClientIp = request.ClientIp
        };

        string authRequest;
        string bankName = "";
        if (_nestPayPos.BankCode == (int)BankCode.Akbank)
        {
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            commonRequest.VisaPfId = _nestPayPos.VisaPfId.Trim();
            authRequest = commonRequest
                .BuildRequestAkbank();

            bankName = Akbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Ziraat)
        {
            commonRequest.SubMerchantId = request.ServiceProviderPspMerchantId;
            authRequest = commonRequest.BuildRequestZiraat();

            bankName = Ziraat;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Halkbank)
        {
            commonRequest.SubMerchantId = request.SubMerchantId;
            authRequest = commonRequest.BuildRequestHalkbank();

            bankName = Halkbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.SekerBank)
        {
            commonRequest.SubMerchantNumber = request.SubMerchantCode;
            authRequest = commonRequest.BuildRequestSekerbank();

            bankName = Seker;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.IsBank)
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            authRequest = commonRequest
                .BuildRequest();

            bankName = IsBank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.TurkiyeFinansKatilim)
        {
            commonRequest.SubMerchantTaxNumber = request.SubMerchantTaxNumber;
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            authRequest = commonRequest.BuildRequestTfkb();

            bankName = Tfkb;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.AnadoluBank)
        {
            authRequest = commonRequest.BuildRequestAnadoluBank();

            bankName = AnadoluBank;
        }
        else
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            authRequest = commonRequest
                .BuildRequest();

            bankName = Nestpay;
        }

        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, authRequest, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPayNonSecureResponse()
          .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ErrorCode;
            response.ResponseMessage = $"{parseResponse.ErrorMessage} - {parseResponse.ErrorDetailMessage}";
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
    }

    public async Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        var expiryDate = FormatExpiryDate(request.ExpireMonth, request.ExpireYear);
        var bonusInquiry = new NestPayBonusInquiryRequest
        {
            MerchantName = _nestPayPos.MerchantName,
            Password = _nestPayPos.Password,
            ClientId = _nestPayPos.ClientId,
            Pan = request.CardNumber,
            Expiry = expiryDate,
            CurrencyCode = request.Currency,
            OrderId = request.OrderNumber,
            Cvv = request.Cvv2,
            QueryName = QueryName,
            OrderType = PaymentDetailOrderStatus
        }.BuildRequest();

        string bankName = "";
        switch (_nestPayPos.BankCode)
        {
            case (int)BankCode.Akbank:
                bankName = Akbank;
                break;
            case (int)BankCode.Ziraat:
                bankName = Ziraat;
                break;
            case (int)BankCode.Halkbank:
                bankName = Halkbank;
                break;
            case (int)BankCode.SekerBank:
                bankName = Seker;
                break;
            case (int)BankCode.IsBank:
                bankName = IsBank;
                break;
            case (int)BankCode.AnadoluBank:
                bankName = AnadoluBank;
                break;
            default:
                bankName = Nestpay;
                break;
        }
        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, bonusInquiry, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPayPointInquiryResponse()
          .Parse(content);

        var response = new PosPointInquiryResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.RrnNumber = parseResponse.TransactionId;
            response.OrderNumber = parseResponse.OrderId;
            response.AvailablePoint = parseResponse.AvailablePoint;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ErrorCode;
            response.ResponseMessage = $"{parseResponse.ErrorMessage} - {parseResponse.DetailMessage}";
        }
        return response;
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        bool postAuthHigher = false;
        if (request.PreAuthAmount < request.Amount)
        {
            postAuthHigher = true;
        }
        var commonRequest = new NestPayPostAuthRequest
        {
            MerchantName = _nestPayPos.MerchantName.Trim(),
            Password = _nestPayPos.Password.Trim(),
            ClientId = _nestPayPos.ClientId.Trim(),
            Amount = FormatAmount(request.Amount),
            PreAuthAmount = FormatAmount(request.PreAuthAmount),
            PostAuthHigher = postAuthHigher,
            CurrencyCode = request.Currency,
            OrderId = request.OrgOrderNumber,
            SubMerchantId = CleanUnicodeCharacters(request.SubMerchantCode),
            SubMerchantName = CleanUnicodeCharacters(request.SubMerchantName),
            SubMerchantCity = CleanUnicodeCharacters(request.SubMerchantCity),
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantCountry = request.SubMerchantCountry,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantTaxNumber = request.SubMerchantTaxNumber,
            Type = nameof(VposTransactionType.PostAuth),
            ClientIp = request.ClientIp,
            IsBlockaged = request.IsBlockaged,
            BlockageCode = request.BlockageCode
        };

        string postAuthRequest;
        string bankName = "";
        if (_nestPayPos.BankCode == (int)BankCode.Akbank)
        {
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            commonRequest.VisaPfId = _nestPayPos.VisaPfId.Trim();
            postAuthRequest = commonRequest
                .BuildRequestAkbank();

            bankName = Akbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Ziraat)
        {
            commonRequest.SubMerchantId = request.ServiceProviderPspMerchantId;
            postAuthRequest = commonRequest.BuildRequestZiraat();

            bankName = Ziraat;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Halkbank)
        {
            commonRequest.SubMerchantId = request.SubMerchantId;
            postAuthRequest = commonRequest.BuildRequestHalkbank();

            bankName = Halkbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.SekerBank)
        {
            commonRequest.SubMerchantNumber = request.SubMerchantCode;
            postAuthRequest = commonRequest.BuildRequestSekerbank();

            bankName = Seker;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.IsBank)
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            postAuthRequest = commonRequest
                .BuildRequest();

            bankName = IsBank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.TurkiyeFinansKatilim)
        {
            commonRequest.SubMerchantTaxNumber = request.SubMerchantTaxNumber;
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            postAuthRequest = commonRequest.BuildRequestTfkb();

            bankName = Tfkb;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.AnadoluBank)
        {
            postAuthRequest = commonRequest.BuildRequestAnadoluBank();

            bankName = AnadoluBank;
        }
        else
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            postAuthRequest = commonRequest
                .BuildRequest();

            bankName = Nestpay;
        }

        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, postAuthRequest, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPayPostAuthResponse()
        .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ErrorCode;
            response.ResponseMessage = $"{parseResponse.ErrorMessage} - {parseResponse.ErrorDetailMessage}";
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var commonRequest = new NestPayRefundRequest
        {
            MerchantName = _nestPayPos.MerchantName.Trim(),
            Password = _nestPayPos.Password.Trim(),
            ClientId = _nestPayPos.ClientId.Trim(),
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrgAuthProcessOrderNo,
            SubMerchantId = CleanUnicodeCharacters(request.SubMerchantCode),
            SubMerchantName = CleanUnicodeCharacters(request.SubMerchantName),
            SubMerchantCity = CleanUnicodeCharacters(request.SubMerchantCity),
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantCountry = request.SubMerchantCountry,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantTaxNumber = request.SubMerchantTaxNumber,
            Type = "Credit",
            ClientIp = request.ClientIp
        };

        string refundRequest;
        string bankName = "";
        if (_nestPayPos.BankCode == (int)BankCode.Akbank)
        {
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            commonRequest.VisaPfId = _nestPayPos.VisaPfId.Trim();
            refundRequest = commonRequest
                .BuildRequestAkbank();

            bankName = Akbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Ziraat)
        {
            commonRequest.SubMerchantId = request.ServiceProviderPspMerchantId;
            refundRequest = commonRequest.BuildRequestZiraat();

            bankName = Ziraat;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Halkbank)
        {
            commonRequest.SubMerchantId = request.SubMerchantId;
            refundRequest = commonRequest.BuildRequestHalkbank();

            bankName = Halkbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.SekerBank)
        {
            commonRequest.SubMerchantNumber = request.SubMerchantCode;
            refundRequest = commonRequest.BuildRequestSekerbank();

            bankName = Seker;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.IsBank)
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            refundRequest = commonRequest
                .BuildRequest();

            bankName = IsBank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.TurkiyeFinansKatilim)
        {
            commonRequest.SubMerchantTaxNumber = request.SubMerchantTaxNumber;
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            refundRequest = commonRequest.BuildRequestTfkb();

            bankName = Tfkb;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.AnadoluBank)
        {
            refundRequest = commonRequest.BuildRequestTfkb();

            bankName = AnadoluBank;
        }
        else
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            refundRequest = commonRequest
                .BuildRequest();

            bankName = Nestpay;
        }

        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, refundRequest, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPayRefundResponse()
        .Parse(content);

        var response = new PosRefundResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ErrorCode;
            response.ResponseMessage = $"{parseResponse.ErrorMessage} - {parseResponse.ErrorDetailMessage}";
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
    }

    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _nestPayPos = (NestPayPosInfo)serviceParameters;
    }

    public async Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string, string> form)
    {
        var response = new PosVerify3dModelResponse();
        response.IsSuccess = false;

        if (form == null)
        {
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Form";
            return await Task.FromResult(response);
        }
        var formString = string.Join(", ", form.Select(kv => $"{kv.Key}: {kv.Value}"));

        form.TryGetValue("mdStatus", out string mdStatus);
        form.TryGetValue("mdErrorMsg", out string mdErrorMessage);
        if (string.IsNullOrEmpty(mdStatus) || !MdStatusCodes.Contains(mdStatus))
        {
            response.ResponseCode = "";
            response.ResponseMessage = mdErrorMessage;
           // return await Task.FromResult(response);
        }

        form.TryGetValue("HASH", out string hash);
        //var decodeHash = HttpUtility.UrlDecode(hash);

        var formHash = GetHash(form);
        form.TryGetValue("oid", out string orderId);  

        if (!formHash.Equals(hash))
        {
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Hash";
        }

        form.TryGetValue("md", out string md);
        form.TryGetValue("xid", out string payerTxnId);
        form.TryGetValue("cavv", out string payerAuthenticationCode);
        form.TryGetValue("eci", out string eci);

        if (mdStatus?.Trim() == "1")
            response.TxnStat = ThreeDFullSecureStatus;
        else if(mdStatus?.Trim() == "2" || mdStatus?.Trim() == "3" || mdStatus?.Trim() == "4")
            response.TxnStat = ThreeDHalfSecureStatus;      

        response.Hash = hash;
        response.Cavv = payerAuthenticationCode;
        response.Eci = eci;
        response.PayerTxnId = payerTxnId;
        response.OrderNumber = orderId;
        response.MD = md;
        response.MdStatus = mdStatus;
        response.MdErrorMessage = mdErrorMessage;
        response.IsSuccess = true;

        return await Task.FromResult(response);
    }
    public async Task<PosVoidResponse> Void(PosVoidRequest request) 
    {
        var commonRequest = new NestPayVoidRequest
        {
            MerchantName = _nestPayPos.MerchantName.Trim(),
            Password = _nestPayPos.Password.Trim(),
            ClientId = _nestPayPos.ClientId.Trim(),
            Amount = FormatAmount(request.Amount),
            CurrencyCode = request.Currency,
            OrderId = request.OrgAuthProcessOrderNo,
            SubMerchantId = CleanUnicodeCharacters(request.SubMerchantCode),
            SubMerchantName = CleanUnicodeCharacters(request.SubMerchantName),
            SubMerchantCity = CleanUnicodeCharacters(request.SubMerchantCity),
            SubMerchantPostalCode = request.SubMerchantPostalCode,
            SubMerchantCountry = request.SubMerchantCountry,
            SubMerchantMcc = request.SubMerchantMcc,
            SubMerchantTaxNumber = request.SubMerchantTaxNumber,
            Type = nameof(VposTransactionType.Void),
            ClientIp = request.ClientIp
        };

        string voidRequest;
        string bankName;
        if (_nestPayPos.BankCode == (int)BankCode.Akbank)
        {
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            commonRequest.VisaPfId = _nestPayPos.VisaPfId.Trim();
            voidRequest = commonRequest
                .BuildRequestAkbank();
            bankName = Akbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Ziraat)
        {
            commonRequest.SubMerchantId = request.ServiceProviderPspMerchantId;
            voidRequest = commonRequest.BuildRequestZiraat();

            bankName = Ziraat;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.Halkbank)
        {
            commonRequest.SubMerchantId = request.SubMerchantId;
            voidRequest = commonRequest.BuildRequestHalkbank();

            bankName = Halkbank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.SekerBank)
        {
            commonRequest.SubMerchantNumber = request.SubMerchantCode;
            voidRequest = commonRequest.BuildRequestSekerbank();

            bankName = Seker;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.IsBank)
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            voidRequest = commonRequest
                .BuildRequest();

            bankName = IsBank;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.TurkiyeFinansKatilim)
        {
            commonRequest.SubMerchantTaxNumber = request.SubMerchantTaxNumber;
            commonRequest.VisaSubmerchantId = _nestPayPos.VisaSubmerchantPfId.Trim();
            voidRequest = commonRequest.BuildRequestTfkb();

            bankName = Tfkb;
        }
        else if (_nestPayPos.BankCode == (int)BankCode.AnadoluBank)
        {
            voidRequest = commonRequest.BuildRequestTfkb();

            bankName = AnadoluBank;
        }
        else
        {
            commonRequest.SubMerchantUrl = CleanUnicodeCharacters(request.SubMerchantUrl);
            commonRequest.SubMerchantGlobalMerchantId = request.SubMerchantGlobalMerchantId;
            voidRequest = commonRequest
                .BuildRequest();

            bankName = Nestpay;
        }

        var content = await SendRequestAsync(_nestPayPos.NonSecureUrl, voidRequest, _nestPayPos.BankCode, bankName);

        var parseResponse = new NestPayVoidResponse()
        .Parse(content);

        var response = new PosVoidResponse();

        if (parseResponse.ResultCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResultCode;
            response.ResponseMessage = parseResponse.ResultDetail;
            response.AuthCode = parseResponse.AuthCode;
            response.TransId = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ErrorCode;
            response.ResponseMessage = $"{parseResponse.ErrorMessage} - {parseResponse.ErrorDetailMessage}";
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
    }

    protected override string FormatAmount(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
           .ToString(new CultureInfo("en-US"));
    }

    protected override string FormatExpiryDate(string month, string year)
    {
        var date = $"{month}.{year}";
        return date;
    }
    private async Task<string> SendRequestAsync(string url, string data, int bankCode, string bankName)
    {
        var correlationId = Guid.NewGuid();

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        await SendIntegrationRequest(data, correlationId, IntegrationLogDataType.Soap, bankName);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var encoding = bankCode == (int)BankCode.SekerBank 
            ? new StringContent(data, Encoding.GetEncoding("ISO-8859-9"), "text/xml") 
            : new StringContent(data, Encoding.UTF8, "text/xml");

        var response = await client.PostAsync(url, encoding);

        await SendIntegrationResponse(response, correlationId, bankName);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
    private async Task SendIntegrationRequest(string data, Guid correlationId, IntegrationLogDataType integrationLogDataType, string bankName)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.NestPayVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = bankName,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Request = data,
                    DataType = integrationLogDataType
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {bankName} - Exception {exception}");
        }

    }
    private async Task SendIntegrationResponse(HttpResponseMessage httpResponse, Guid correlationId, string bankName)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.NestPayVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = bankName,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Response = await httpResponse.Content.ReadAsStringAsync(),
                    HttpCode = ((int)httpResponse.StatusCode).ToString(),
                    ErrorCode = httpResponse.StatusCode.ToString(),
                    ErrorMessage = httpResponse.StatusCode.ToString(),
                    DataType = IntegrationLogDataType.Soap
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {bankName} - Exception {exception}");
        }

    }
}
