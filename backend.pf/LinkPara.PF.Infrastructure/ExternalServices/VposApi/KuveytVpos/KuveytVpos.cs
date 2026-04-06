using KuveytServiceReference;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IntegrationLogger;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Request;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Response;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos;

public class KuveytVpos : VposBase, IVposApi
{
    private readonly string ThreeDFullSecureStatus = "Y";
    private const string SuccessCode = "00";
    private const string APIVersion = "TDV2.0.0";
    private const string NonSecureAPIVersion = " 1.0.0 ";
    private const string TransactionSecurity = "1";
    private const string ThreedTransactionSecurity = "3";

    private KuveytPosInfo _kuveytPosInfo;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly ILogger<KuveytVpos> _logger;
    private readonly IVirtualPosService _vposPosService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IVaultClient _vaultClient;

    public KuveytVpos(
        IParameterService parameterService,
        IBus bus,
        ILogger<KuveytVpos> logger,
        IServiceProvider serviceProvider,
        IVaultClient vaultClient)
    {
        _parameterService = parameterService;
        _bus = bus;
        _vaultClient = vaultClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
        var client = new VirtualPosServiceClient(GetBinding(), GetEndpointAddress());
        var isLogEnable = GetParameterValue().GetAwaiter().GetResult();
        if (isLogEnable == VposConsts.EnableLogValue)
        {
            var inspectorBehavior = _serviceProvider.GetRequiredService<SoapInspectorBehavior>();
            inspectorBehavior.Name = nameof(VposConsts.KuveytVpos);
            inspectorBehavior.Type = nameof(IntegrationLogType.Vpos);
            client.Endpoint.EndpointBehaviors.Add(inspectorBehavior);
        }
        _vposPosService = client;
    }
    public async Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        if (request.AuthType == VposAuthType.PreAuth)
        {
            return await PreAuthAsync(request);
        }

        var authRequest = new KuveytPaymentNonSecureRequest
        {
            MerchantId = _kuveytPosInfo.MerchantId,
            CustomerId = _kuveytPosInfo.CustomerId,
            UserName = _kuveytPosInfo.UserName,
            CardHolderName = request.CardHolderName,
            Pan = request.CardNumber,
            ExpireMonth = request.ExpireMonth,
            ExpireYear = request.ExpireYear,
            Amount = amount,
            Currency = $"0{request.Currency}",
            PFSubMerchantId = request.SubMerchantId,
            TransactionSecurity = TransactionSecurity,
            HashPassword = hashPassword,
            HashData = hashData,
            MerchantOrderId = request.OrderNumber,
            BKMId = request.SubMerchantGlobalMerchantId,
            LanguageCode = request.LanguageCode,
            InstallmentCount = request.Installment ?? 0,
            Cvv2 = request.Cvv2,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            VposSubMerchantId = request.SubMerchantCode,
            TransactionType = "Sale",
            APIVersion = NonSecureAPIVersion
        }.BuildRequest();

        var content = await SendRequestAsync(_kuveytPosInfo.NonSecureUrl, authRequest);

        var parseResponse = new KuveytNonSecureResponse()
         .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.RrnNumber = parseResponse.RRN;
            response.AuthCode = parseResponse.ProvisionNumber;
            response.Stan = parseResponse.Stan;
            response.TransId = request.OrderNumber;
            response.OrderNumber = parseResponse.OrderId;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
    }
    private async Task<PosPaymentProvisionResponse> PreAuthAsync(PosPaymentNonSecureRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        var submerchantData = new PFSubMerchantData
        {
            PFSubMerchantId = request.SubMerchantId,
            BKMId = request.SubMerchantGlobalMerchantId,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            PFSubMerchantTerminalId = request.SubMerchantCode,

        };
        var vposMessage = new KuveytTurkVPosMessage
        {
            TransactionType = "PreAuthorization",
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            UserName = _kuveytPosInfo.UserName,
            HashData = hashData,
            HashPassword = hashPassword,
            MerchantOrderId = request.OrderNumber,
            CardNumber = request.CardNumber,
            CardExpireDateMonth = request.ExpireMonth,
            CardExpireDateYear = request.ExpireYear,
            CardCVV2 = request.Cvv2,
            CardHolderName = request.CardHolderName,
            CardType = GetCardBrand(request.CardBrand),
            Amount = Convert.ToInt32(request.Amount * 100m),
            CurrencyCode = $"0{request.Currency}",
            InstallmentCount = request.Installment ?? 0,
            PFSubMerchantData = submerchantData
        };
        var preAuthRequest = new VPosManuelPreAuthTransactionRequest
        {
            VPosMessage = vposMessage
        };

        var result = await _vposPosService.PreAuthorizationAsync(preAuthRequest);

        var response = new PosPaymentProvisionResponse();

        if (result.Success && result.Value.ResponseCode == SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = result.Value.ResponseCode;
            response.ResponseMessage = result.Value.ResponseMessage;
            response.RrnNumber = result.Value.RRN;
            response.Stan = result.Value.Stan;
            response.AuthCode = result.Value.ProvisionNumber;
            response.TransId = request.OrderNumber;
            response.OrderNumber = result.Value.OrderId.ToString();
            response.TrxDate = result.Value.TransactionTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.Results[0].ErrorCode;
            response.ResponseMessage = result.Results[0].ErrorMessage;
            response.TrxDate = DateTime.Now;
        }

        return response;
    }

    public async Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
    {
        if (request.AuthType == VposAuthType.PreAuth)
        {
            return new PosInit3DModelResponse { IsSuccess = false, HtmlContent = string.Empty };
        }

        var url = $"{request.VposCallbackUrl}/session/{request.ThreedSessionId}/{request.OrderNumber}";
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrderNumber, amount, url, url, _kuveytPosInfo.UserName, hashPassword);

        var threedRequest = new KuveytInit3dModelRequest
        {
            MerchantId = _kuveytPosInfo.MerchantId,
            CustomerId = _kuveytPosInfo.CustomerId,
            UserName = _kuveytPosInfo.UserName,
            CardHolderName = request.CardHolderName,
            Pan = request.CardNumber,
            ExpireMonth = request.ExpireMonth,
            ExpireYear = request.ExpireYear,
            Amount = amount,
            Currency = $"0{request.Currency}",
            PFSubMerchantId = request.SubMerchantId,
            TransactionSecurity = ThreedTransactionSecurity,
            FailUrl = url,
            OkUrl = url,
            HashData = hashData,
            MerchantOrderId = request.OrderNumber,
            BKMId = request.SubMerchantGlobalMerchantId,
            LanguageCode = request.LanguageCode,
            InstallmentCount = request.Installment,
            Cvv2 = request.Cvv2,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            VposSubMerchantId = request.SubMerchantCode,
            TransactionType = "Sale",
            APIVersion = APIVersion,
            ClientIpAddress = request.ClientIp,
            SubmerchantCity = request.SubMerchantCity,
            SubmerchantCountry = request.SubMerchantCountry,
            SubmerchantAddress = request.SubmerchantAddress,
            SubmerchantPostalCode = request.SubMerchantPostalCode,
            SubmerchantDistrict = request.SubmerchantDistrict,
            SubmerchantEmail = request.SubmerchantEmail,
            SubmerchantPhoneCode = request.SubmerchantPhoneCode,
            SubmerchantPhoneNumber = request.SubmerchantPhoneNumber
        }.BuildRequest();

        var content = await SendRequestAsync(_kuveytPosInfo.EnrollmentUrl, threedRequest);

        if (string.IsNullOrEmpty(content))
        {
            return new PosInit3DModelResponse { IsSuccess = false, HtmlContent = string.Empty };
        }

        return new PosInit3DModelResponse
        {
            IsSuccess = true,
            HtmlContent = Base64Encode(content),
            ResponseCode = string.Empty,
            ResponseMessage = string.Empty,
        };
    }

    public async Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
    {
        if (request.AuthType == VposAuthType.PreAuth)
        {
            return new PosPaymentProvisionResponse { IsSuccess = false, ResponseCode = "99", ResponseMessage = "Ön Provizyon işlemine izin verilmemektedir." };
        }

        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrgOrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        var authRequest = new KuveytPayment3dModelRequest
        {
            MerchantId = _kuveytPosInfo.MerchantId,
            CustomerId = _kuveytPosInfo.CustomerId,
            UserName = _kuveytPosInfo.UserName,
            Amount = amount,
            TransactionSecurity = ThreedTransactionSecurity,
            HashData = hashData,
            MerchantOrderId = request.OrgOrderNumber,
            InstallmentCount = request.Installment ?? 0,
            MD = request.MD,
            TransactionType = "Sale",
            APIVersion = APIVersion
        }.BuildRequest();

        var content = await SendRequestAsync(_kuveytPosInfo.ThreeDSecureUrl, authRequest);

        var parseResponse = new KuveytNonSecureResponse()
         .Parse(content);

        var response = new PosPaymentProvisionResponse();

        if (parseResponse.ResponseCode is SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.RrnNumber = parseResponse.RRN;
            response.AuthCode = parseResponse.ProvisionNumber;
            response.Stan = parseResponse.Stan;
            response.OrderNumber = parseResponse.OrderId;
            response.TransId = request.OrderNumber;
            response.TrxDate = parseResponse.TrxDate;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = parseResponse.ResponseCode;
            response.ResponseMessage = parseResponse.ResponseMessage;
            response.TrxDate = parseResponse.TrxDate;
        }
        return response;
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

        var authenticationResponse = form["AuthenticationResponse"].ToString();
        if (string.IsNullOrEmpty(authenticationResponse))
        {
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Form";
            return await Task.FromResult(response);
        }

        authenticationResponse = HttpUtility.UrlDecode(authenticationResponse);

        var serializer = new XmlSerializer(typeof(VPosTransactionResponseContract));

        var model = new VPosTransactionResponseContract();
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(authenticationResponse)))
        {
            model = serializer.Deserialize(ms) as VPosTransactionResponseContract;
        }

        if (model.ResponseCode != "00")
        {
            response.ResponseCode = model.ResponseCode;
            response.ResponseMessage = model.ResponseMessage;
            return await Task.FromResult(response);
        }

        var merchantOrderId = model.MerchantOrderId;

        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(merchantOrderId, model.ResponseCode, model.OrderId.ToString(), hashPassword);

        if (!model.HashData.Equals(hashData))
        {
            response.Hash = model.HashData;
            response.ResponseCode = "";
            response.ResponseMessage = "Invalid Hash";
            return await Task.FromResult(response);
        }

        response.MdStatus = model.ResponseCode == "00" ? "1" : "0";
        response.TxnStat = model.ResponseCode == "00" ? ThreeDFullSecureStatus : "N"; 
        response.Hash = model.HashData;
        response.OrderNumber = model.OrderId.ToString();
        response.MD = model.MD;
        response.IsSuccess = true;

        return await Task.FromResult(response);
    }

    public async Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrgOrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        var submerchantData = new PFSubMerchantData
        {
            PFSubMerchantId = request.SubMerchantId,
            BKMId = request.SubMerchantGlobalMerchantId,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            PFSubMerchantTerminalId = request.SubMerchantCode,

        };
        var vposMessage = new KuveytTurkVPosMessage
        {
            TransactionType = "PreAuthClose",
            CurrencyCode = $"0{request.Currency}",
            HashData = hashData,
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            UserName = _kuveytPosInfo.UserName,
            Amount = Convert.ToInt32(request.Amount * 100m),
            MerchantOrderId = request.OrgOrderNumber,
            HashPassword = hashPassword,
            PFSubMerchantData = submerchantData,

        };
        var postAuthRequest = new VPosManuelClosePreAuthTransactionRequest
        {
            OriginalRRN = request.RRN,
            OriginalStan = request.Stan,
            ProvisionNumber = request.ProvisionNumber,
            message = vposMessage,
            OrderId = Convert.ToInt32(request.BankOrderId),

        };
        var result = await _vposPosService.PreAuthorizationCloseAsync(postAuthRequest);

        var response = new PosPaymentProvisionResponse();

        if (result.Success && result.Value.ResponseCode == SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = result.Value.ResponseCode;
            response.ResponseMessage = result.Value.ResponseMessage;
            response.RrnNumber = result.Value.RRN;
            response.Stan = result.Value.Stan;
            response.AuthCode = result.Value.ProvisionNumber;
            response.OrderNumber = result.Value.OrderId.ToString();
            response.TransId = request.OrderNumber;
            response.TrxDate = result.Value.TransactionTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.Results[0].ErrorCode;
            response.ResponseMessage = result.Results[0].ErrorMessage;
            response.TrxDate = DateTime.Now;
        }

        return response;
    }

    public async Task<PosVoidResponse> Void(PosVoidRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrgOrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        if (request.TransactionType == TransactionType.PreAuth)
        {
            return await PreAuthVoidAsync(request);
        }

        var submerchantData = new PFSubMerchantData
        {
            PFSubMerchantId = request.SubMerchantId,
            BKMId = request.SubMerchantGlobalMerchantId,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            PFSubMerchantTerminalId = request.SubMerchantCode,

        };
        var vposMessage = new KuveytTurkVPosMessage
        {
            UserName = _kuveytPosInfo.UserName,
            CurrencyCode = $"0{request.Currency}",
            HashData = hashData,
            HashPassword = hashPassword,
            TransactionType = "SaleReversal",
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            Amount = Convert.ToInt32(request.Amount * 100m),
            MerchantOrderId = request.OrgOrderNumber,
            PFSubMerchantData = submerchantData

        };
        var voidRequest = new VPosManuelSaleReversalTransactionRequest
        {
            RRN = request.RRN,
            Stan = request.Stan,
            ProvisionNumber = request.ProvisionNumber,
            VPosMessage = vposMessage,
            OrderId = Convert.ToInt32(request.BankOrderId),

        };

        var result = await _vposPosService.SaleReversalAsync(voidRequest);

        var response = new PosVoidResponse();

        if (result.Success && result.Value.ResponseCode == SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = result.Value.ResponseCode;
            response.ResponseMessage = result.Value.ResponseMessage;
            response.RrnNumber = result.Value.RRN;
            response.Stan = result.Value.Stan;
            response.AuthCode = result.Value.ProvisionNumber;
            response.OrderNumber = result.Value.OrderId.ToString();
            response.TransId = request.OrderNumber;
            response.TrxDate = result.Value.TransactionTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.Results[0].ErrorCode;
            response.ResponseMessage = result.Results[0].ErrorMessage;
            response.TrxDate = DateTime.Now;
        }

        return response;
    }

    private async Task<PosVoidResponse> PreAuthVoidAsync(PosVoidRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrgOrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        var submerchantData = new PFSubMerchantData
        {
            PFSubMerchantId = request.SubMerchantId,
            BKMId = request.SubMerchantGlobalMerchantId,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            PFSubMerchantTerminalId = request.SubMerchantCode,

        };
        var vposMessage = new KuveytTurkVPosMessage
        {
            UserName = _kuveytPosInfo.UserName,
            CurrencyCode = $"0{request.Currency}",
            HashData = hashData,
            HashPassword = hashPassword,
            TransactionType = " PreAuthorizationClose",
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            Amount = Convert.ToInt32(request.Amount * 100m),
            MerchantOrderId = request.OrgOrderNumber,
            PFSubMerchantData = submerchantData

        };
        var preAuthVoidRequest = new VPosManuelPreAuthReversalTransactionRequest
        {
            RRN = request.RRN,
            Stan = request.Stan,
            ProvisionNumber = request.ProvisionNumber,
            VPosMessage = vposMessage,
            OrderId = Convert.ToInt32(request.BankOrderId),

        };
        var result = await _vposPosService.PreAuthorizationReversalAsync(preAuthVoidRequest);

        var response = new PosVoidResponse();

        if (result.Success && result.Value.ResponseCode == SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = result.Value.ResponseCode;
            response.ResponseMessage = result.Value.ResponseMessage;
            response.RrnNumber = result.Value.RRN;
            response.Stan = result.Value.Stan;
            response.AuthCode = result.Value.ProvisionNumber;
            response.OrderNumber = result.Value.OrderId.ToString();
            response.TransId = request.OrderNumber;
            response.TrxDate = result.Value.TransactionTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.Results[0].ErrorCode;
            response.ResponseMessage = result.Results[0].ErrorMessage;
            response.TrxDate = DateTime.Now;
        }

        return response;
    }

    public async Task<PosRefundResponse> Refund(PosRefundRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrgOrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        if (request.Amount < request.TotalAmount)
        {
            return await PartialRefundAsync(request);
        }

        var submerchantData = new PFSubMerchantData
        {
            PFSubMerchantId = request.SubMerchantId,
            BKMId = request.SubMerchantGlobalMerchantId,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            PFSubMerchantTerminalId = request.SubMerchantCode,

        };
        var vposMessage = new KuveytTurkVPosMessage
        {
            TransactionType = "Drawback",
            CurrencyCode = $"0{request.Currency}",
            HashData = hashData,
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            UserName = _kuveytPosInfo.UserName,
            Amount = Convert.ToInt32(request.Amount * 100m),
            HashPassword = hashPassword,
            MerchantOrderId = request.OrgOrderNumber,
            PFSubMerchantData = submerchantData,
        };

        var refundRequest = new VPosManuelDrawBackTransactionRequest
        {
            RRN = request.RRN,
            Stan = request.Stan,
            VPosMessage = vposMessage,
            OrderId = Convert.ToInt32(request.BankOrderId),

        };
        var result = await _vposPosService.DrawBackAsync(refundRequest);

        var response = new PosRefundResponse();

        if (result.Success && result.Value.ResponseCode == SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = result.Value.ResponseCode;
            response.ResponseMessage = result.Value.ResponseMessage;
            response.RrnNumber = result.Value.RRN;
            response.Stan = result.Value.Stan;
            response.AuthCode = result.Value.ProvisionNumber;
            response.OrderNumber = result.Value.OrderId.ToString();
            response.TransId = request.OrderNumber;
            response.TrxDate = result.Value.TransactionTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.Results[0].ErrorCode;
            response.ResponseMessage = result.Results[0].ErrorMessage;
            response.TrxDate = DateTime.Now;
        }

        return response;
    }

    private async Task<PosRefundResponse> PartialRefundAsync(PosRefundRequest request)
    {
        var amount = FormatAmount(request.Amount);
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrgOrderNumber, amount, _kuveytPosInfo.UserName, hashPassword);

        var submerchantData = new PFSubMerchantData
        {

            PFSubMerchantId = request.SubMerchantId,
            BKMId = request.SubMerchantGlobalMerchantId,
            PFSubMerchantIdentityTaxNumber = request.SubMerchantTaxNumber,
            PFSubMerchantTerminalId = request.SubMerchantCode,

        };
        var vposMessage = new KuveytTurkVPosMessage
        {
            TransactionType = "PartialDrawback",
            CurrencyCode = $"0{request.Currency}",
            HashData = hashData,
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            UserName = _kuveytPosInfo.UserName,
            Amount = Convert.ToInt32(request.Amount * 100m),
            HashPassword = hashPassword,
            MerchantOrderId = request.OrgOrderNumber,
            PFSubMerchantData = submerchantData,
        };

        var partialRefundRequest = new VPosManuelDrawBackTransactionRequest
        {
            RRN = request.RRN,
            Stan = request.Stan,
            VPosMessage = vposMessage,
            OrderId = Convert.ToInt32(request.BankOrderId),

        };

        var result = await _vposPosService.PartialDrawbackAsync(partialRefundRequest);

        var response = new PosRefundResponse();

        if (result.Success && result.Value.ResponseCode == SuccessCode)
        {
            response.IsSuccess = true;
            response.ResponseCode = result.Value.ResponseCode;
            response.ResponseMessage = result.Value.ResponseMessage;
            response.RrnNumber = result.Value.RRN;
            response.Stan = result.Value.Stan;
            response.AuthCode = result.Value.ProvisionNumber;
            response.OrderNumber = result.Value.OrderId.ToString();
            response.TransId = request.OrderNumber;
            response.TrxDate = result.Value.TransactionTime;
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.Results[0].ErrorCode;
            response.ResponseMessage = result.Results[0].ErrorMessage;
            response.TrxDate = DateTime.Now;
        }

        return response;
    }

    public async Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
    {
        var hashPassword = GetHashPassword(_kuveytPosInfo.Password);
        var hashData = GetSha1(_kuveytPosInfo.MerchantId, request.OrderNumber, "0", _kuveytPosInfo.UserName, hashPassword);

        var vposMessage = new KuveytTurkVPosMessage
        {
            MerchantId = Convert.ToInt32(_kuveytPosInfo.MerchantId),
            CustomerId = Convert.ToInt32(_kuveytPosInfo.CustomerId),
            UserName = _kuveytPosInfo.UserName,
            HashData = hashData,
            HashPassword = hashPassword,
            MerchantOrderId = request.OrderNumber
        };
        var paymentDetailRequest = new OrderRequest
        {
            VPosMessage = vposMessage,
            MerchantOrderId = request.OrderNumber
        };

        var result = await _vposPosService.GetMerchantOrderDetailAsync(paymentDetailRequest);

        var response = new PosPaymentDetailResponse();

        if (result.Success is true)
        {
            response.IsSuccess = true;
            if (result.Value.Length == 0)
            {
                response.OrderStatus = OrderStatus.OrderNotFound;
            }
            else
            {
                var orderResult = result.Value[0];

                response.ResponseCode = orderResult.ResponseCode;
                response.ResponseMessage = "";
                response.AuthCode = orderResult.ProvNumber;
                response.TransId = orderResult.OrderId.ToString();
                response.TransactionDate = orderResult.OrderDate;
                response.Amount = orderResult.FirstAmount;
                response.TrxDate = orderResult.OrderDate;
                response.RefundedAmount = orderResult.DrawbackAmount ?? 0;
                response.RrnNumber = orderResult.RRN;
                response.BatchNo = orderResult.BatchId.ToString();
                response.Stan = orderResult.Stan;
                response.CardInformation = orderResult.CardHolderName;
                response.OrderStatus = GetOrderStatus(orderResult);
            }
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = result.ErrorCode;
            response.ResponseMessage = result.ErrorMessage;
            response.OrderStatus = OrderStatus.Unknown;
        }

        return response;
    }
    private static OrderStatus GetOrderStatus(OrderContract response)
    {
        return response.LastOrderStatus switch
        {
            8 => OrderStatus.Rejected,
            6 => OrderStatus.Cancelled,
            10 => OrderStatus.Cancelled,
            1 => OrderStatus.WaitingEndOfDay,
            4 => OrderStatus.Refunded,
            5 => OrderStatus.Refunded,
            2 => OrderStatus.PreAuth,
            3 => OrderStatus.PostAuth,
            9 => OrderStatus.EndOfDayCompleted,
            _ => OrderStatus.Unknown
        };
    }
    public Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
    {
        throw new NotImplementedException();
    }
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
    private async Task<string> SendRequestAsync(string url, string data)
    {
        var correlationId = Guid.NewGuid();

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        await SendIntegrationRequest(data, correlationId);

        var response = await client.PostAsync(url, new StringContent(data, Encoding.UTF8, "text/xml"));

        await SendIntegrationResponse(response, correlationId);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }
    private async Task SendIntegrationRequest(string data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.KuveytVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.KuveytVpos,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Request = data,
                    DataType = IntegrationLogDataType.Soap
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.KuveytVpos} - Exception {exception}");
        }

    }
    private async Task SendIntegrationResponse(HttpResponseMessage data, Guid correlationId)
    {
        try
        {
            var isLogEnable = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.KuveytVpos);
            if (isLogEnable.ParameterValue == VposConsts.EnableLogValue)
            {

                var bytes = await data.Content.ReadAsByteArrayAsync();
                var log = new IntegrationLog()
                {
                    CorrelationId = correlationId.ToString(),
                    Name = VposConsts.KuveytVpos,
                    Type = nameof(IntegrationLogType.Vpos),
                    Date = DateTime.Now,
                    Response = Encoding.UTF8.GetString(bytes),
                    DataType = IntegrationLogDataType.Soap
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SendToRawDataQueue Error: VposName {VposConsts.KuveytVpos} - Exception {exception}");
        }

    }
    private static string GetSha1(string merchantId, string merchantOrderId, string amount, string userName, string hashPassword)
    {
        var provider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(provider);

        var cryptoServiceProvider = new SHA1CryptoServiceProvider();

        var hashString = $"{merchantId}{merchantOrderId}{amount}{userName}{hashPassword}";
        var hashBytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(hashString);

        return Convert.ToBase64String(cryptoServiceProvider.ComputeHash(hashBytes));
    }
    private static string GetSha1(string merchantOrderId, string responseCode, string orderId, string hashPassword)
    {
        var provider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(provider);

        var cryptoServiceProvider = new SHA1CryptoServiceProvider();

        var hashString = $"{merchantOrderId}{responseCode}{orderId}{hashPassword}";
        var hashBytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(hashString);

        return Convert.ToBase64String(cryptoServiceProvider.ComputeHash(hashBytes));
    }
    private static string GetSha1(string merchantId, string merchantOrderId, string amount, string okUrl, string failUrl, string userName, string hashPassword)
    {
        var provider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(provider);

        var cryptoServiceProvider = new SHA1CryptoServiceProvider();

        var hashString = $"{merchantId}{merchantOrderId}{amount}{okUrl}{failUrl}{userName}{hashPassword}";
        var hashBytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(hashString);

        return Convert.ToBase64String(cryptoServiceProvider.ComputeHash(hashBytes));
    }
    private static string GetHashPassword(string password)
    {
        var provider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(provider);

        var cryptoServiceProvider = new SHA1CryptoServiceProvider();
        var inputBytes = cryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(inputBytes);
    }
    private static string GetCardBrand(CardBrand cardBrand)
    {
        return cardBrand switch
        {
            CardBrand.Visa => "VISA",
            CardBrand.Troy => "TROY",
            CardBrand.MasterCard => "MasterCard",
            _ => "VISA",
        };
    }
    private async Task<string> GetParameterValue()
    {
        var response = await _parameterService.GetParameterAsync
            (VposConsts.ParameterGroupCode, VposConsts.KuveytVpos);
        return response.ParameterValue;
    }
    private EndpointAddress GetEndpointAddress()
    {
        return new EndpointAddress(_vaultClient.GetSecretValue<string>("PFSecrets", "Kuveyt", "ServiceSoapUrl"));
    }
    private Binding GetBinding()
    {
        BasicHttpsBinding result = new BasicHttpsBinding();
        result.MaxBufferSize = int.MaxValue;
        result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
        result.MaxReceivedMessageSize = int.MaxValue;
        result.AllowCookies = true;
        return result;
    }
    protected override string FormatAmount(decimal amount)
    {
        return Convert.ToInt32(amount * 100m).ToString();
    }

    protected override string FormatExpiryDate(string month, string year)
    {
        if (year.Length > 2)
        {
            return $"{month}{year.Substring(2, 2)}";
        }

        return $"{month}{year}";
    }
    public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
    {
        _kuveytPosInfo = (KuveytPosInfo)serviceParameters;
    }
    public class VPosTransactionResponseContract
    {
        public string ACSURL { get; set; }
        public string OkUrl { get; set; }
        public string FailUrl { get; set; }
        public string AuthenticationPacket { get; set; }
        public string HashData { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsSuccess { get; }
        public bool IsVirtual { get; set; }
        public string MD { get; set; }
        public string MerchantOrderId { get; set; }
        public int OrderId { get; set; }
        public string PareqHtmlFormString { get; set; }
        public string Password { get; set; }
        public string ProvisionNumber { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string RRN { get; set; }
        public string SafeKey { get; set; }
        public string Stan { get; set; }
        public DateTime TransactionTime { get; set; }
        public string TransactionType { get; set; }
        public KuveytTurkVPosMessages VPosMessage { get; set; }
    }
    public class KuveytTurkVPosMessages
    {
        public decimal Amount { get; set; }
    }
}
