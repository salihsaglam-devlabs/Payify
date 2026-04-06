using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;

using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Commands.CreateSecurityQuestion;

public class CreateSecurityQuestionCommand : IRequest, IMapFrom<SecurityQuestion>
{
    public string Question { get; set; }
    public string LanguageCode { get; set; }
}

public class CreateSecurityQuestionCommandHandler : IRequestHandler<CreateSecurityQuestionCommand>
{
    private readonly IRepository<SecurityQuestion> _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly IApplicationUserService _applicationUserService;
    public CreateSecurityQuestionCommandHandler(IRepository<SecurityQuestion> repository,
       IAuditLogService auditLogService,
       IMapper mapper,
       IApplicationUserService applicationUserService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _applicationUserService = applicationUserService;   
    }

    public async Task<Unit> Handle(CreateSecurityQuestionCommand command, CancellationToken cancellationToken)
    {
        var securityQuestion =_mapper.Map<SecurityQuestion>(command);

        if (await IsExistAsync(securityQuestion))
            throw new DuplicateRecordException();

        await _repository.AddAsync(securityQuestion);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateSecurityQuestion",
                SourceApplication = "Identity",
                Resource = "SecurityQuestion",
                UserId = _applicationUserService.ApplicationUserId,
                Details = new Dictionary<string, string>
                {
                     {"Question", command.Question },
                     {"LanguageCode", command.LanguageCode },
                }
            });

        return Unit.Value;
    }

    private async Task<bool> IsExistAsync(SecurityQuestion securityQuestion)
    {
        return await _repository.GetAll().AnyAsync(x =>
            x.Question == securityQuestion.Question &&
            x.LanguageCode == securityQuestion.LanguageCode &&
            x.RecordStatus == RecordStatus.Active
        );
    }
}
