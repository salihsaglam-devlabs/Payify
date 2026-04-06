using LinkPara.Emoney.Application.Commons.Models.ConsentModels.Responses;
using LinkPara.Emoney.Application.Features.ConsentOperations;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.CancelConsent;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.UpdateConsent;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetActiveConsentList;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetConsentDetail;
using LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetWaitingApprovalConsents;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IConsentOperationsService
{
    Task<List<ConsentDto>> GetActiveConsentListAsync(GetActiveConsentListQuery query);
    Task<CancelConsentResultDto> CancelConsentAsync(CancelConsentCommand command);
    Task<GetWaitingApprovalConsentResponse> GetWaitingApprovalConsentsAsync(GetWaitingApprovalConsentQuery query);
    Task<GetConsentDetailResponse> GetConsentDetailAsync(GetConsentDetailQuery query);
    Task<UpdateConsentResultDto> UpdateConsentAsync(UpdateConsentCommand command);
    Task<List<GetControlBalanceConsentResponse>> GetConsentsForChangeBalanceControl();
}
