using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetAllSubMerchantUsers;

public class GetAllSubMerchantUserQuery : SearchQueryParams, IRequest<PaginatedList<SubMerchantUserDto>>
{
    public Guid? UserId { get; set; }
    public string Fullname { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string IdentityNumber { get; set; }
    public Guid? SubMerchantId { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllSubMerchantUserQueryHandler : IRequestHandler<GetAllSubMerchantUserQuery, PaginatedList<SubMerchantUserDto>>
{
    private readonly ISubMerchantUserService _subMerchantUserService;

    public GetAllSubMerchantUserQueryHandler(ISubMerchantUserService subMerchantUserService)
    {
        _subMerchantUserService = subMerchantUserService;
    }
    public async Task<PaginatedList<SubMerchantUserDto>> Handle(GetAllSubMerchantUserQuery query, CancellationToken cancellationToken)
    {
        return await _subMerchantUserService.GetListAsync(query);
    }
}
