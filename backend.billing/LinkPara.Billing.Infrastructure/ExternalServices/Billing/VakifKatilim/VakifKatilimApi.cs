using System.ServiceModel;
using System.Text.Json;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Infrastructure.CorporationPaymentsService;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Interfaces;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Http;

namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim;

public class VakifKatilimApi : IVakifKatilimApi
{
    private readonly IGenericRepository<InstitutionMapping> _institutionMappingRepository;
    private readonly string _baseUrl;
    private readonly CorporationSecurityContext _corporationSecurityContext;
    private readonly IIntegrationLogger _integrationLogger;
    private const int MaxReceivedMessageSize = 4194304;

    public VakifKatilimApi(
        IGenericRepository<InstitutionMapping> institutionMappingRepository,
        IVaultClient vaultClient, 
        IIntegrationLogger integrationLogger)
    {
        _institutionMappingRepository = institutionMappingRepository;
        _integrationLogger = integrationLogger;
        _baseUrl =
            vaultClient.GetSecretValue<string>("/BillingSecrets", "VakifKatilim", "BaseUrl");
        _corporationSecurityContext =
            vaultClient.GetSecretValue<CorporationSecurityContext>("/BillingSecrets", "VakifKatilim");
    }


    public async Task<LinkPara.Billing.Infrastructure.CorporationPaymentsService.InstitutionList[]> GetInstitutionListAsync()
    {
        var request = new RequestInstitutionList
        {
            CorporationSecurityContext = _corporationSecurityContext
        };
        
        var correlationId = await LogIntegrationAsync(request, "GetInstitutionListAsync", null);
        
        var res = await GetServiceClient()
            .GetCorporationsAsync(request);
        
        await LogIntegrationAsync(res, "GetInstitutionListAsync", correlationId);


        return res.Success ? res.InstitutionList : Array.Empty<LinkPara.Billing.Infrastructure.CorporationPaymentsService.InstitutionList>();
    }

    public async Task<ResponseBillInquiry> InquireBillsAsync(int institutionId, string firstQueryField, Vendor vendor)
    {
        var request = new RequestBillInquiry
        {
            CorporationSecurityContext = _corporationSecurityContext,
            FirstQueryField = firstQueryField,
            InstitutionId = institutionId,
            SecondQueryField = null

        };
        
        var correlationId = await LogIntegrationAsync(request, "InquireBillsAsync", null);
        
        var res = await GetServiceClient()
            .GetDebtAsync(request);

        await LogIntegrationAsync(res, "InquireBillsAsync", correlationId);
        
        return res;
    }

    public async Task<ResponseBillCancel> CancelPaymentAsync(int paymentId)
    {
        var request = new RequestBillCancel
        {
            CorporationSecurityContext = _corporationSecurityContext,
            TransactionGuidId = paymentId
        };
        
        var correlationId = await LogIntegrationAsync(request, "CancelPaymentAsync", null);
        
        var res = await GetServiceClient()
            .CancelPaymentAsync(request);

        await LogIntegrationAsync(res, "CancelPaymentAsync", correlationId);
        
        return res;
    }

    public async Task<ResponseBillControl> ControlPaymentAsync(int paymentId)
    {
        var request = new RequestBillControl
        {
            CorporationSecurityContext = _corporationSecurityContext,
            TransactionGuidId = paymentId
        };
        
        var correlationId = await LogIntegrationAsync(request, "ControlPaymentAsync", null);
        
        var res = await GetServiceClient()
            .ControlPaymentAsync(request);

        await LogIntegrationAsync(res, "ControlPaymentAsync", correlationId);
        
        return res;
    }

    public async Task<ResponseBillPayment> DoPaymentAsync(int paymentId, BillList bill, PayInquiredBillCommand payInquiredBillCommand, string identityNumber)
    {
        var request = new RequestBillPayment
        {
            CorporationSecurityContext = _corporationSecurityContext,
            PayerName = payInquiredBillCommand.PayeeFullName,
            PayerPhone = payInquiredBillCommand.PayeeMobile,
            PayerTaxNumber = identityNumber,
            TransactionGuidId = paymentId,
            BillList = new[]
            {
                new BillList
                {
                    Amount = bill.Amount,
                    BillNumber = bill.BillNumber,
                    DebtAmount = bill.DebtAmount,
                    DebtId = bill.DebtId,
                    InstitutionId = bill.InstitutionId,
                    InvoiceDelayAmount = bill.InvoiceDelayAmount,
                    LastPaymenDate = bill.LastPaymenDate,
                    NameSurname = bill.NameSurname,
                    Period = bill.Period,
                    SubscriberNumber = bill.SubscriberNumber,
                }
            },
        };
        
        var correlationId = await LogIntegrationAsync(request, "DoPaymentAsync", null);
        
        var res = await GetServiceClient()
            .DoPaymentAsync(request);
        
        await LogIntegrationAsync(res, "DoPaymentAsync", correlationId);

        return res;
    }

    public async Task<ResponseReconciliationSummary> ReconciliationSummaryAsync(DateTime dateTime)
    {
        return await ReconciliationSummaryByInstitutionAsync(dateTime, null);
    }
    
    public async Task<ResponseReconciliationSummary> ReconciliationSummaryByInstitutionAsync(DateTime dateTime, int? institutionId)
    {
        var request = new RequestReconciliationSummary
        {
            CorporationSecurityContext = _corporationSecurityContext,
            GroupByInstitution = institutionId.HasValue,
            ReconciliationDate = dateTime,
            InstitutionId = institutionId
        };
        
        var correlationId = await LogIntegrationAsync(request, "ReconciliationSummaryByInstitutionAsync", null);
        
        var res = await GetServiceClient()
            .ReconciliationSummaryAsync(request);

        await LogIntegrationAsync(res, "ReconciliationSummaryByInstitutionAsync", correlationId);
        
        return res;
    }

    public async Task<ResponseReconciliationDetail> ReconcilationSummaryDetailAsync(DateTime dateTime,
        int institutionId, bool getOnlyFaultyTransactions)
    {
        var request = new RequestReconciliationDetail
        {
            CorporationSecurityContext = _corporationSecurityContext,
            GetOnlyFaultyTransactions = getOnlyFaultyTransactions,
            InstitutionId = institutionId,
            ReconciliationDate = dateTime,
        };
        
        var correlationId = await LogIntegrationAsync(request, "ReconcilationSummaryDetailAsync", null);
        
        var res = await GetServiceClient()
            .ReconciliationSummaryDetailAsync(request);

        await LogIntegrationAsync(res, "ReconcilationSummaryDetailAsync", correlationId);
        
        return res;
    }

    private CorporationPaymentsServiceClient GetServiceClient()
    {
        var endpointAddress = new EndpointAddress(_baseUrl);

        var httpsBinding = new BasicHttpBinding
        {
            Security = new BasicHttpSecurity
            {
                Mode = BasicHttpSecurityMode.Transport
            },
            MaxReceivedMessageSize = MaxReceivedMessageSize
        };

        var serviceClient = new CorporationPaymentsServiceClient(httpsBinding, endpointAddress);
        
        return serviceClient;
    }
    
    private async Task<string> LogIntegrationAsync(object data, string methodName, string correlationId)
    {
        var isRequest = false;
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            isRequest = true;
        }
        
        await _integrationLogger.QueueLogAsync(new IntegrationLog
        {
            CorrelationId = correlationId,
            MethodName = methodName,
            Name = "Billing",
            Type = "VakifKatilim",
            Request = isRequest ?  JsonSerializer.Serialize(data) : null,
            Response = isRequest ? null : JsonSerializer.Serialize(data),
            Date = DateTime.Now,
            HttpCode = HttpMethods.Post,
            DataType = IntegrationLogDataType.Soap
        });
    
        return correlationId;
    }
}