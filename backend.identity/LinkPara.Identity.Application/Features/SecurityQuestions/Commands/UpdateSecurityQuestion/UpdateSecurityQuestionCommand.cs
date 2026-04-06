using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using LinkPara.Identity.Application.Common.Mappings;
using AutoMapper;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Commands.UpdateSecurityQuestion;

public class UpdateSecurityQuestionCommand : IRequest, IMapFrom<SecurityQuestion>
{
    public Guid Id { get; set; }
    public string Question { get; set; }
    public string LanguageCode { get; set; }
}

public class UpdateSecurityQuestionCommandHandler : IRequestHandler<UpdateSecurityQuestionCommand>
{
    private readonly IRepository<SecurityQuestion> _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly IApplicationUserService _applicationUserService;

    public UpdateSecurityQuestionCommandHandler(IRepository<SecurityQuestion> repository,
        IAuditLogService auditLogService,
        IMapper mapper,
        IApplicationUserService applicationUserService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _applicationUserService = applicationUserService;   
    }

    public async Task<Unit> Handle(UpdateSecurityQuestionCommand command, CancellationToken cancellationToken)
    {
        var securityQuestion = await _repository.GetAll().SingleOrDefaultAsync(x=>x.Id==command.Id 
        && x.RecordStatus==RecordStatus.Active);

        if (securityQuestion is null)
        {
            throw new NotFoundException(nameof(SecurityQuestion));
        }

        securityQuestion =_mapper.Map(command, securityQuestion);
        await _repository.UpdateAsync(securityQuestion);

        await _auditLogService.AuditLogAsync(
              new AuditLog
              {
                  IsSuccess = true,
                  LogDate = DateTime.Now,
                  Operation = "UpdateSecurityQuestion",
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
}