using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.MerchantContents.Command.CreateMerchantContent;

public class CreateMerchantContentCommand : IRequest
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public List<MerchantContentVersionDto> Contents { get; set; }
}

public class CreateMerchantContentCommandHandler : IRequestHandler<CreateMerchantContentCommand>
{
    private readonly IGenericRepository<MerchantContent> _repository;
    private readonly IGenericRepository<MerchantContentVersion> _merchantContentVersionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<CreateMerchantContentCommandHandler> _logger;

    public CreateMerchantContentCommandHandler(
        IGenericRepository<MerchantContent> repository,
        IGenericRepository<MerchantContentVersion> merchantContentVersionRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        ILogger<CreateMerchantContentCommandHandler> logger
    )
    {
        _repository = repository;
        _merchantContentVersionRepository = merchantContentVersionRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateMerchantContentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
            
            var merchantContent = new MerchantContent
            {
                MerchantId = command.MerchantId,
                Name = command.Name,
                ContentSource = command.ContentSource,
                CreatedBy = parseUserId.ToString()
            };

            await _repository.AddAsync(merchantContent);

            foreach (var merchantContentVersion in 
                     command.Contents.Select(item => new MerchantContentVersion
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
                    Operation = "CreateMerchantContent",
                    SourceApplication = "PF",
                    Resource = "MerchantContent",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Name", command.Name }
                    }
                });

            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "CreateMerchantContentError : {Exception}", exception);
            throw;
        }
    }
}