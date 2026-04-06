using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Command.SaveMerchantIntegrator;

public class SaveMerchantIntegratorCommand : IRequest
{
    public string Name { get; set; }
    public decimal CommissionRate { get; set; }
}

public class SaveMerchantIntegratorCommandHandler : IRequestHandler<SaveMerchantIntegratorCommand>
{
    private readonly IMerchantIntegratorService _merchantIntegratorService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public SaveMerchantIntegratorCommandHandler(IMerchantIntegratorService merchantIntegratorService,
    IAuditLogService auditLogService,
    IContextProvider contextProvider)
    {
        _merchantIntegratorService = merchantIntegratorService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(SaveMerchantIntegratorCommand request, CancellationToken cancellationToken)
    {
        await _merchantIntegratorService.SaveAsync(request);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveMerchantIntegrator",
                SourceApplication = "PF",
                Resource = "MerchantIntegrator",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"Name", request.Name},
                    {"CommissionRate", request.CommissionRate.ToString()},
                }
            });

        return Unit.Value;
    }
}
