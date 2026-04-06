using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.AcquireBanks;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.AcquireBanks.Command.PatchAcquireBank;

public class PatchAcquireBankCommand : IRequest<UpdateAcquireBankRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateAcquireBankRequest> AcquireBank { get; set; }
}

public class PatchAcquireBankCommandHandler : IRequestHandler<PatchAcquireBankCommand, UpdateAcquireBankRequest>
{
    private readonly IGenericRepository<AcquireBank> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<AcquireBank> _logger;
    private readonly IAuditLogService _auditLogService;

    public PatchAcquireBankCommandHandler(IGenericRepository<AcquireBank> repository, 
        IMapper mapper,
        ILogger<AcquireBank> logger,
        IAuditLogService auditLogService
        )
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService; 
    }
    public async Task<UpdateAcquireBankRequest> Handle(PatchAcquireBankCommand request, CancellationToken cancellationToken)
    {
        var acquireBank = await _repository.GetAll()
         .Include(b => b.Bank).FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), request.Id);
        }
        try
        {
            var acquireBankMap = _mapper.Map<UpdateAcquireBankRequest>(acquireBank);

            request.AcquireBank.ApplyTo(acquireBankMap);
            _mapper.Map(acquireBankMap, acquireBank);

            await _repository.UpdateAsync(acquireBank);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateAcquireBank",
                SourceApplication = "PF",
                Resource = "AcquireBank",
                Details = new Dictionary<string, string>
                {
                       {"Id", acquireBank.Id.ToString() },
                       {"BankCode", acquireBankMap.BankCode.ToString() },
                       {"CardNetwork", acquireBankMap.CardNetwork.ToString() },
                       {"EndOfDayHour", acquireBankMap.EndOfDayHour.ToString() },
                       {"EndOfDayMinute", acquireBankMap.EndOfDayMinute.ToString() },
                }
            });

            return acquireBankMap;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "AcquireBankPatchError : {Exception}", exception);
            throw;
        }       
        
    }
}
