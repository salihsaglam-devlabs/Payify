using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Application.Commons.Models.MerchantUser;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantUsers.Queries.GetUserName;

public class GetUserNameQuery : IRequest<MerchantUserNameModel>
{
    public string Identifier { get; set; }
}

public class GetUserNameQueryHandler : IRequestHandler<GetUserNameQuery, MerchantUserNameModel>
{
    private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;

    public GetUserNameQueryHandler(IGenericRepository<MerchantUser> merchantUserRepository, IGenericRepository<SubMerchantUser> subMerchantUserRepository)
    {
        _merchantUserRepository = merchantUserRepository;
        _subMerchantUserRepository = subMerchantUserRepository;
    }
    public async Task<MerchantUserNameModel> Handle(GetUserNameQuery request, CancellationToken cancellationToken)
    {
        var merchantUserNameModel = new MerchantUserNameModel { UserName = string.Empty };
        
        var checkMerchantUser = await _merchantUserRepository.GetAll()
            .Where(s => request.Identifier.Contains(s.MobilePhoneNumber) && s.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (checkMerchantUser is not null)
        {
            merchantUserNameModel.UserName = string.Concat(UserTypePrefix.Corporate, request.Identifier.Replace("+", ""));
        }
        else
        {
            var checkSubMerchantUser = await _subMerchantUserRepository.GetAll()
                .Where(s => request.Identifier.Contains(s.MobilePhoneNumber) && s.RecordStatus == RecordStatus.Active)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (checkSubMerchantUser is not null)
            {
                merchantUserNameModel.UserName = string.Concat(UserTypePrefix.CorporateSubMerchant, request.Identifier.Replace("+", ""));
            }
        }

        if (string.IsNullOrEmpty(merchantUserNameModel.UserName))
        {
            throw new NotFoundException(nameof(MerchantUser), request.Identifier);
        }

        return merchantUserNameModel;
    }
}
