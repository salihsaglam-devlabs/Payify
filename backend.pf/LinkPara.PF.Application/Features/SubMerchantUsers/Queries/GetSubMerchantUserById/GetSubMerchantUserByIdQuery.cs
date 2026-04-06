using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetSubMerchantUserById;

public class GetSubMerchantUserByIdQuery : IRequest<SubMerchantUserDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantUserByIdQueryHandler : IRequestHandler<GetSubMerchantUserByIdQuery, SubMerchantUserDto>
{
    private readonly ISubMerchantUserService _subMerchantUserService;

    public GetMerchantUserByIdQueryHandler(ISubMerchantUserService subMerchantUserService)
    {
        _subMerchantUserService = subMerchantUserService;
    }
    public async Task<SubMerchantUserDto> Handle(GetSubMerchantUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantUserService.GetByIdAsync(request.Id);
    }
}
