using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantUsers.Queries.GetAllMerchantUser;

public class GetAllMerchantUserQuery : SearchQueryParams, IRequest<PaginatedList<MerchantUserDto>>
{
    public Guid? UserId { get; set; }
    public string Fullname { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string RoleId { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllMerchantUserQueryHandler : IRequestHandler<GetAllMerchantUserQuery, PaginatedList<MerchantUserDto>>
{
    private readonly IMerchantUserService _merchantUserService;

    public GetAllMerchantUserQueryHandler(IMerchantUserService merchantUserService)
    {
        _merchantUserService = merchantUserService;
    }
    public async Task<PaginatedList<MerchantUserDto>> Handle(GetAllMerchantUserQuery request, CancellationToken cancellationToken)
    {
        return await _merchantUserService.GetAllAsync(request);
    }
}
