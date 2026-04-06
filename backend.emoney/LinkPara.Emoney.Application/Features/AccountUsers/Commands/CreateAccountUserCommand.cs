using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountUsers.Commands;

public class CreateAccountUserCommand : IRequest
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
}

public class CreateAccountUserCommandHandler : IRequestHandler<CreateAccountUserCommand>
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;

    public CreateAccountUserCommandHandler(
        IGenericRepository<AccountUser> accountUserRepository,
        IGenericRepository<Account> accountRepository,
        IContextProvider contextProvider,
        IAuditLogService auditLogService)
    {
        _accountUserRepository = accountUserRepository;
        _accountRepository = accountRepository;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(CreateAccountUserCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAll()
            .AnyAsync(s =>
                s.Id == request.AccountId &&
                s.RecordStatus == RecordStatus.Active);

        if (!account)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        var duplicateCheck = await _accountUserRepository.GetAll()
            .AnyAsync(s =>
                s.UserId == request.UserId &&
                s.RecordStatus == RecordStatus.Active &&
                (s.Email == request.Email.ToLowerInvariant() || (s.PhoneCode == request.PhoneCode && s.PhoneNumber == request.PhoneNumber)));

        if (duplicateCheck)
        {
            throw new DuplicateRecordException();
        }

        var user = new AccountUser
        {
            AccountId = request.AccountId,
            CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
            UserId = request.UserId,
            Email = request.Email.ToLowerInvariant(),
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            Firstname = request.Firstname,
            Lastname = request.Lastname            
        };

        await _accountUserRepository.AddAsync(user);

        var details = new Dictionary<string, string>
        {
            {"Firstname", request.Firstname},
            {"Lastname", request.Lastname },
            {"Email", request.Email },
            {"Phone", string.Concat(request.PhoneCode, request.PhoneNumber) },
            {"UserId", request.UserId.ToString() },
            {"AccountId", request.AccountId.ToString() },
        };

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "SaveAccountUser",
                Resource = "AccountUser",
                SourceApplication = "Emoney",
                UserId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            }
        );

        return Unit.Value;

    }
}
