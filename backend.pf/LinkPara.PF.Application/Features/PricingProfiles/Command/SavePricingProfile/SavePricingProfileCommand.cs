using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.SavePricingProfile;

public class SavePricingProfileCommand : IRequest, IMapFrom<PricingProfile>
{
    public string Name { get; set; }
    public ProfileType ProfileType { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PerTransactionFee { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}

public class SavePricingProfileCommandHandler : IRequestHandler<SavePricingProfileCommand>
{
    private readonly IPricingProfileService _pricingProfileService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public SavePricingProfileCommandHandler(IPricingProfileService pricingProfileService,
    IAuditLogService auditLogService,
    IContextProvider contextProvider)
    {
        _pricingProfileService = pricingProfileService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }
    public async Task<Unit> Handle(SavePricingProfileCommand request, CancellationToken cancellationToken)
    {
        await _pricingProfileService.SaveAsync(request);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SavePricingProfile",
                SourceApplication = "PF",
                Resource = "PricingProfile",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"Name", request.Name},
                    {"ActivationDate", request.ActivationDate.ToString()},
                    {"PerTransactionFee", request.PerTransactionFee.ToString()},
                }
            });

        return Unit.Value;
    }
}
