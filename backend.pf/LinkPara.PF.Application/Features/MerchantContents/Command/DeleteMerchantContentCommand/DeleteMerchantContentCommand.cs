using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.MerchantContents.Command.DeleteMerchantContentCommand;

public class DeleteMerchantContentCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteMerchantContentCommandHandler : IRequestHandler<DeleteMerchantContentCommand>
{
    private readonly IGenericRepository<MerchantContent> _repository;
    private readonly IGenericRepository<MerchantContentVersion> _merchantContentVersionRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<DeleteMerchantContentCommandHandler> _logger;

    public DeleteMerchantContentCommandHandler(
        IGenericRepository<MerchantContent> repository,
        IGenericRepository<MerchantContentVersion> merchantContentVersionRepository,
        IContextProvider contextProvider,
        IAuditLogService auditLogService,
        ILogger<DeleteMerchantContentCommandHandler> logger)
    {
        _repository = repository;
        _merchantContentVersionRepository = merchantContentVersionRepository;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteMerchantContentCommand command, CancellationToken cancellationToken)
    {
        var merchantContent = await _repository
            .GetAll()
            .Include(l => l.Contents)
            .FirstOrDefaultAsync(l => l.Id == command.Id, cancellationToken: cancellationToken);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (merchantContent is null)
        {
            throw new NotFoundException(nameof(MerchantContent), command.Id);
        }

        try
        {
            merchantContent.RecordStatus = RecordStatus.Passive;
            merchantContent.LastModifiedBy = parseUserId.ToString();
            merchantContent.UpdateDate = DateTime.Now;

            await _repository.UpdateAsync(merchantContent);

            foreach (var contentVersion in merchantContent.Contents)
            {
                contentVersion.RecordStatus = RecordStatus.Passive;
                contentVersion.LastModifiedBy = parseUserId.ToString();
                contentVersion.UpdateDate = DateTime.Now;
                await _merchantContentVersionRepository.UpdateAsync(contentVersion);
            }

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteMerchantContent",
                    SourceApplication = "PF",
                    Resource = "MerchantContent",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString() },
                    }
                });
            
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "MerchantContentDeleteError : {Exception}", exception);
            throw;
        }
    }
}
