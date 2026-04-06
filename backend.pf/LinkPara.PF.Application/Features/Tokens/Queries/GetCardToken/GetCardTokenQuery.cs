using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Tokens.Queries.GetCardToken;

public class GetCardTokenQuery : IRequest<CardTokenDto>
{
    public string ThreeDSessionId { get; set; }
}

public class GetCardTokenQueryHandler : IRequestHandler<GetCardTokenQuery, CardTokenDto>
{
    private readonly IGenericRepository<ThreeDVerification> _threeDVerificationRepository;
    private readonly IAuditLogService _auditLogService;

    public GetCardTokenQueryHandler(
            IGenericRepository<ThreeDVerification> threeDVerificationRepository,
            IAuditLogService auditLogService)
    {
        _threeDVerificationRepository = threeDVerificationRepository;
        _auditLogService = auditLogService;
    }

    public async Task<CardTokenDto> Handle(GetCardTokenQuery request,
        CancellationToken cancellationToken)
    {
        var verification = await _threeDVerificationRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == Guid.Parse(request.ThreeDSessionId) &&
                                      s.RecordStatus == RecordStatus.Active &&
                                      s.CurrentStep == VerificationStep.VerificationFinished, cancellationToken: cancellationToken);

        if (verification is null)
        {
            throw new NotFoundException(nameof(ThreeDVerification), request.ThreeDSessionId);
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "GetCardToken",
                SourceApplication = "PF",
                Resource = "CardToken",
                Details = new Dictionary<string, string>
                {
                    {"Token", verification.CardToken},
                    {"ThreeDSessionId", verification.Id.ToString()}
                }
            });

        return new CardTokenDto
        {
            CardToken = verification.CardToken
        };
    }
}