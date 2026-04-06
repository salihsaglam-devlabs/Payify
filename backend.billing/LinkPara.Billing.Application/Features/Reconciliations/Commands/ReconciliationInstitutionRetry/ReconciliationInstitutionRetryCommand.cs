using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Domain.Enums;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationInstitutionRetry;

public class ReconciliationInstitutionRetryCommand : IRequest<RetryReconciliationInstitutionResponseDto>
{
    public Guid InstitutionSummaryId { get; set; }
}

public class ReconciliationInstitutionRetryCommandHandler : IRequestHandler<ReconciliationInstitutionRetryCommand, RetryReconciliationInstitutionResponseDto>
{
    private readonly IReconciliationService _reconciliationService;
    private readonly IBillingService _billingService;

    public ReconciliationInstitutionRetryCommandHandler(IReconciliationService reconciliationService, IBillingService billingService)
    {
        _reconciliationService = reconciliationService;
        _billingService = billingService;
    }

    public async Task<RetryReconciliationInstitutionResponseDto> Handle(ReconciliationInstitutionRetryCommand request, CancellationToken cancellationToken)
    {
        var institutionSummary = await _reconciliationService.GetInstitutionSummaryByIdAsync(request.InstitutionSummaryId);

        if (institutionSummary.ReconciliationStatus == ReconciliationStatus.Success)
        {
            throw new InvalidOperationException($"ReconciliationIsAlreadySuccessful");
        }

        var response = new RetryReconciliationInstitutionResponseDto
        {
            VendorName = institutionSummary.Vendor.Name,
            InstitutionName = institutionSummary.Institution.Name,
            ReconciliationStatus = ReconciliationStatus.Success,
            ReconciliationDate = institutionSummary.ReconciliationDate
        };

        var reconciliationSummaryRequest = new ReconciliationSummaryRequest
        {
            ReconciliationDate = institutionSummary.ReconciliationDate,
            VendorId = institutionSummary.VendorId
        };
        var reconciliationSummaryResponse = await _billingService.GetReconciliationSummaryAsync(reconciliationSummaryRequest);

        if (!reconciliationSummaryResponse.IsSuccess)
        {
            throw new ReconciliationSummaryException("ErrorPerformingReconciliationSummary");
        }

        var institutionSummaryRequest = new ReconciliationDetailsRequest
        {
            InstitutionId = institutionSummary.InstitutionId,
            VendorId = institutionSummary.VendorId,
            ReconciliationDate = institutionSummary.ReconciliationDate
        };
        var institutionSummaryResponse = await _billingService.GetReconciliationDetailsAsync(institutionSummaryRequest);

        if (!institutionSummaryResponse.IsSuccess)
        {
            throw new ReconciliationDetailException("ErrorPerformingReconciliationDetail");
        }

        if (!institutionSummaryResponse.Response.ReconciliationDetails.Any(r => r.ReconciliationStatus == ReconciliationStatus.Fail))
        {
            var reconciliationCloseResponse = await _billingService.CloseInstitutionReconciliationAsync(new InstitutionReconciliationCloseRequest
            {
                ReconciliationDate = institutionSummary.ReconciliationDate,
                VendorId = institutionSummary.VendorId,
                InstitutionReconciliations = institutionSummaryResponse.Response.ReconciliationDetails
            });

            if (!reconciliationCloseResponse.IsSuccess)
            {
                response.ReconciliationStatus = ReconciliationStatus.Fail;
                response.Description = reconciliationCloseResponse.ErrorMessage;
            }
        }
        else
        {
            var institutionSummaryId = institutionSummaryResponse.Response.ReconciliationDetails.FirstOrDefault().InstitutionSummaryId;
            await _billingService.GetInstitutionPaymentDetailsAsync(institutionSummaryId);

            response.ReconciliationStatus = ReconciliationStatus.Fail;

            throw new ReconciliationCloseException("CannotCloseReconciliationDueToCoflictingRecords");
        }

        return response;
    }
}