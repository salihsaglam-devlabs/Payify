using LinkPara.HttpProviders.Approval.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.HttpProviders.Approval
{
    public interface IRequestsService
    {
        Task<PaginatedList<RequestCashbackDto>> GetAllCashbackRequestsAsync([FromQuery] GetCashbackRequestsQuery query);
        Task<PaginatedList<RequestWalletBlockageDto>> GetAllWalletBlocakgeRequestsAsync([FromQuery] GetWalletBlockageRequestsQuery query);
    }
}