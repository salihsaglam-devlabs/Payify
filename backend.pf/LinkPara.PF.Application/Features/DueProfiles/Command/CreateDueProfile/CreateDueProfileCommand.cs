using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Features.DueProfiles.Command.CreateDueProfile
{
    public class CreateDueProfileCommand : IRequest
    {
        public string Title { get; set; }
        public DueType DueType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public TimeInterval OccurenceInterval { get; set; }
    }

    public class CreateDueProfileCommandHandler : IRequestHandler<CreateDueProfileCommand>
    {
        private readonly IGenericRepository<DueProfile> _repository;
        private readonly IAuditLogService _auditLogService;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger<CreateDueProfileCommand> _logger;

        public CreateDueProfileCommandHandler(
            IGenericRepository<DueProfile> repository,
            IAuditLogService auditLogService,
            IContextProvider contextProvider,
            ILogger<CreateDueProfileCommand> logger
        )
        {
            _repository = repository;
            _auditLogService = auditLogService;
            _contextProvider = contextProvider;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateDueProfileCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var isDueProfileAlreadyExist = await _repository.GetAll()
                    .AnyAsync(d => d.Title == command.Title 
                    && d.DueType == command.DueType 
                    && d.OccurenceInterval == command.OccurenceInterval, cancellationToken: cancellationToken);

                if (isDueProfileAlreadyExist)
                {
                    throw new DuplicateRecordException();
                }

                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

                var dueProfile = new DueProfile
                {
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    DueType = command.DueType,
                    Amount = command.Amount,
                    Currency = command.Currency,
                    OccurenceInterval = command.OccurenceInterval,
                    IsDefault = false,
                    RecordStatus = RecordStatus.Active,
                    CreatedBy = parseUserId.ToString()
                };

                await _repository.AddAsync(dueProfile);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "CreateDueProfile",
                        SourceApplication = "PF",
                        Resource = "DueProfile",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                            {"Title", command.Title },
                            {"DueType", command.DueType.ToString() }
                        }
                    });

                return Unit.Value;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "CreateDueProfileError : {Exception}", exception);
                throw;
            }
        }
    }
}
