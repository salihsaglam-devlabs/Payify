using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Commons.Models.LogoConfiguration;
using LinkPara.Accounting.Application.Commons.Models.LogoResponse;
using LinkPara.Accounting.Application.Features.Invoice;
using LinkPara.Accounting.Application.Features.Invoice.Queries.GetInvoice;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace LinkPara.Accounting.Infrastructure.Services.AccountingServices;

public class TigerInvoiceService : IInvoiceService
{
    private readonly HttpClient _client;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IVaultClient _vaultClient;

    public TigerInvoiceService(HttpClient client, IVaultClient vaultClient, IGenericRepository<Payment> paymentRepository)
    {
        _client = client;
        _vaultClient = vaultClient;
        _paymentRepository = paymentRepository;
    }

    public async Task<InvoiceDto> GetPaymentBillAsync(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository
            .GetAll()
            .SingleOrDefaultAsync(p => p.ClientReferenceId == request.PaymentClientReferenceId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("PaymentWithClientReferenceIdNotFound: {PaymentClientReferenceId}", request.PaymentClientReferenceId);
        }

        var logoSettings = _vaultClient.GetSecretValue<LogoSettings>("AccountingSecrets", "LogoSettings");
        var requestUri = $"api/InvoiceLink?user={logoSettings.User}&pass={logoSettings.Password}&firmNo={logoSettings.FirmNo}&OPR_TYPE=6&REFERANCE={payment.ReferenceId}";
        var response = await _client.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var exceptionDetails = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new Exception(exceptionDetails);
        }

        var billResponse = await response.Content.ReadFromJsonAsync<ServiceResponse>();
        var linkItem = billResponse?.linkList?.FirstOrDefault();

        return new InvoiceDto
        {
            IsSuccess = billResponse.isSuccess,
            BillUrl = linkItem?.invoiceLink
        };
    }
}