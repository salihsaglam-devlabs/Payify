using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.SubMerchants.Queries.GetSubMerchantSummary;

public class GetSubMerchantSummaryQuery : IRequest<SubMerchantSummaryDto>
{
}

public class GetSubMerchantSummaryQueryHandler : IRequestHandler<GetSubMerchantSummaryQuery, SubMerchantSummaryDto>
{
    private readonly IGenericRepository<SubMerchant> _repository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IRestrictionService _restrictionService;
    private readonly IMapper _mapper;
    
    public GetSubMerchantSummaryQueryHandler(
        IGenericRepository<SubMerchant> repository, 
        IGenericRepository<SubMerchantUser> subMerchantRepository, 
        IGenericRepository<Merchant> merchantRepository,
        IContextProvider contextProvider,
        IMapper mapper,
        IRestrictionService restrictionService)
    {
        _repository = repository;
        _subMerchantUserRepository = subMerchantRepository;
        _merchantRepository = merchantRepository;
        _contextProvider = contextProvider;
        _mapper = mapper;
        _restrictionService = restrictionService;
    }

    public async Task<SubMerchantSummaryDto> Handle(GetSubMerchantSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
        
        var subMerchantUser = await _subMerchantUserRepository.GetAll()
            .FirstOrDefaultAsync(s => s.UserId == parseUserId, cancellationToken);
        
        var subMerchant = await _repository.GetAll()
            .Select(s => new SubMerchantSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Number = s.Number,
                MerchantType = s.MerchantType,
                MerchantId = s.MerchantId,
                City = s.City,
                CityName = s.CityName,
                IsManuelPaymentPageAllowed = s.IsManuelPaymentPageAllowed,
                IsLinkPaymentPageAllowed = s.IsLinkPaymentPageAllowed,
                IsOnUsPaymentPageAllowed = s.IsOnUsPaymentPageAllowed,
                PreAuthorizationAllowed = s.PreAuthorizationAllowed,
                PaymentReverseAllowed = s.PaymentReverseAllowed,
                PaymentReturnAllowed = s.PaymentReturnAllowed,
                InstallmentAllowed = s.InstallmentAllowed,
                Is3dRequired = s.Is3dRequired,
                IsExcessReturnAllowed = s.IsExcessReturnAllowed,
                InternationalCardAllowed = s.InternationalCardAllowed,
                RejectReason = s.RejectReason,
                CreateDate = s.CreateDate,
                RecordStatus = s.RecordStatus
            })
            .FirstOrDefaultAsync(s => s.Id == subMerchantUser.SubMerchantId, cancellationToken);

        await _restrictionService.IsUserAuthorizedAsync(subMerchant.MerchantId);
        await _restrictionService.RestrictMerchantTypes(new List<MerchantType> { MerchantType.StandartMerchant });
        
        var merchant = await _merchantRepository.GetAll()
            .Include(m => m.MerchantApiKeyList)
            .FirstOrDefaultAsync(m => m.Id == subMerchant.MerchantId, cancellationToken);
        
        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), subMerchant.MerchantId);
        }
        
        var subMerchantSummaryDto = _mapper.Map<SubMerchantSummaryDto>(subMerchant);

        subMerchantSummaryDto.MerchantNumber = merchant.Number;
        subMerchantSummaryDto.MerchantName = merchant.Name;
        subMerchantSummaryDto.IntegrationMode = merchant.IntegrationMode;
        subMerchantSummaryDto.PublicKey = merchant.MerchantApiKeyList.FirstOrDefault()?.PublicKey;

        return subMerchantSummaryDto;
    }
}