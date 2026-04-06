using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.MerchantContents.Command.PutMerchantContent;

public class PutMerchantContentCommand : IRequest
{
    public MerchantContentDto MerchantContent { get; set; }
}

public class PutMerchantContentCommandHandler : IRequestHandler<PutMerchantContentCommand>
{
    private readonly IGenericRepository<MerchantContent> _repository;
    private readonly IGenericRepository<MerchantContentVersion> _merchantContentVersionRepository;
    private readonly ILogger<PutMerchantContentCommandHandler> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public PutMerchantContentCommandHandler(
        IGenericRepository<MerchantContent> repository,
        IGenericRepository<MerchantContentVersion> merchantContentVersionRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        ILogger<PutMerchantContentCommandHandler> logger)
    {
        _repository = repository;
        _merchantContentVersionRepository = merchantContentVersionRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(PutMerchantContentCommand command, CancellationToken cancellationToken)
    {
        var merchantContent = await _repository.GetAll()
            .Include(x => x.Contents.Where(a => a.RecordStatus == RecordStatus.Active))
            .Where(x => x.Id == command.MerchantContent.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (merchantContent is null)
        {
            throw new NotFoundException(nameof(MerchantContent), command.MerchantContent.Id);
        }
        
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            merchantContent.MerchantId = command.MerchantContent.MerchantId;
            merchantContent.Name = command.MerchantContent.Name;
            merchantContent.ContentSource = command.MerchantContent.ContentSource;
            merchantContent.LastModifiedBy = parseUserId.ToString();
            merchantContent.UpdateDate = DateTime.Now;
            await _repository.UpdateAsync(merchantContent);

            foreach (var contentVersion in merchantContent.Contents)
            {
                contentVersion.RecordStatus = RecordStatus.Passive;
                await _merchantContentVersionRepository.UpdateAsync(contentVersion);
            }
            
            foreach (var merchantContentVersion in 
                     command.MerchantContent.Contents.Select(item => new MerchantContentVersion
                     {
                         MerchantContentId = merchantContent.Id,
                         Title = item.Title,
                         Content = item.Content,
                         LanguageCode = item.LanguageCode,
                         CreatedBy = parseUserId.ToString()
                     }))
            {
                await _merchantContentVersionRepository.AddAsync(merchantContentVersion);
            }
            
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "PutMerchantContent",
                    SourceApplication = "PF",
                    Resource = "MerchantContent",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.MerchantContent.Id.ToString() },
                        {"Name", command.MerchantContent.Name }
                    }
                });
            
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "PutMerchantContentError : {Exception}", exception);
            throw;
        }
    }
}
