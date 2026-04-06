using AutoMapper;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultiplePricingProfile;

public class UpdateMultiplePricingProfileCommand : IRequest, IMapFrom<Merchant>
{
    public List<Guid> MainSubMerchantIds { get; set; }
    public string PricingProfileNumber { get; set; }
}
public class UpdateMultiplePricingProfileCommandHandler : IRequestHandler<UpdateMultiplePricingProfileCommand>
{
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly ILogger<UpdateMultiplePricingProfileCommandHandler> _logger;

    public UpdateMultiplePricingProfileCommandHandler(
        IGenericRepository<Merchant> merchantRepository,
        IContextProvider contextProvider,
        IMapper mapper,
        IAuditLogService auditLogService,
        ILogger<UpdateMultiplePricingProfileCommandHandler> logger,
        IStringLocalizerFactory factory,
        IGenericRepository<PricingProfile> pricingProfileRepository = null)
    {
        _merchantRepository = merchantRepository;
        _logger = logger;
        _pricingProfileRepository = pricingProfileRepository;
    }
    public async Task<Unit> Handle(UpdateMultiplePricingProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var pricingProfile = await _pricingProfileRepository
                .GetAll().Where(b => b.PricingProfileNumber == request.PricingProfileNumber && b.ProfileStatus == ProfileStatus.InUse)
                .FirstOrDefaultAsync();

            if (pricingProfile is null)
            {
                throw new NotFoundException(nameof(PricingProfile), request.PricingProfileNumber);
            }

            foreach (var merchantId in request.MainSubMerchantIds)
            {
                var merchant = await _merchantRepository.GetByIdAsync(merchantId);
                if (merchant is null)
                    throw new NotFoundException(nameof(Merchant), merchantId);

                if (ShouldUpdatePricingProfile(pricingProfile.ProfileType, merchant.MerchantType))
                {
                    if (merchant.IsPaymentToMainMerchant != pricingProfile.IsPaymentToMainMerchant)
                    {
                        throw new InvalidMerchantPricingProfileException();
                    }
                    merchant.PricingProfileNumber = request.PricingProfileNumber;
                    await _merchantRepository.UpdateAsync(merchant);
                }
            }

            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateMultiplePricingProfileCommandError : {exception}");
            throw;
        }
    }
    private bool ShouldUpdatePricingProfile(ProfileType profileType, MerchantType merchantType)
    {
        return (profileType == ProfileType.Standard &&
                (merchantType == MerchantType.StandartMerchant || merchantType == MerchantType.MainMerchant || merchantType == MerchantType.EasyMerchant))
            || (profileType == ProfileType.SubMerchant && merchantType == MerchantType.SubMerchant);
    }
}