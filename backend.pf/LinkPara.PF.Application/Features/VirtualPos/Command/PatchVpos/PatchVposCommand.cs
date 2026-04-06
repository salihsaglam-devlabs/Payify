using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.PatchVpos;

public class PatchVposCommand : IRequest<UpdateVposRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateVposRequest> Vpos { get; set; }
}

public class PatchVposCommandHandler : IRequestHandler<PatchVposCommand, UpdateVposRequest>
{
    private readonly IGenericRepository<Vpos> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<Vpos> _logger;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IAuditLogService _auditLogService;
    public PatchVposCommandHandler(IGenericRepository<Vpos> repository, IMapper mapper,
        ILogger<Vpos> logger, IGenericRepository<MerchantVpos> merchantVposRepository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _merchantVposRepository = merchantVposRepository;
        _auditLogService= auditLogService;  
    }
    public async Task<UpdateVposRequest> Handle(PatchVposCommand request, CancellationToken cancellationToken)
    {
        var vpos = await _repository.GetAll().Include(b => b.VposBankApiInfos)
            .ThenInclude(c => c.Key).Include(b => b.AcquireBank.Bank).Include(b => b.CostProfiles)
            .ThenInclude(b => b.CostProfileItems.OrderBy(b => b.InstallmentNumberEnd))
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (vpos is null)
        {
            throw new NotFoundException(nameof(Vpos), request.Id);
        }

        try
        {
            var vposMap = _mapper.Map<UpdateVposRequest>(vpos);

            request.Vpos.ApplyTo(vposMap);

            _mapper.Map(vposMap, vpos);

            if ((vpos.IsOnUsVpos && (vpos.IsInsuranceVpos || vpos.IsTopUpVpos == true)) ||
                (vpos.IsInsuranceVpos && (vpos.IsOnUsVpos || vpos.IsTopUpVpos == true)) ||
                (vpos.IsTopUpVpos == true && (vpos.IsInsuranceVpos || vpos.IsOnUsVpos)))
            {
                throw new MultipleVposTypeException();
            }
            
            if (vpos.IsOnUsVpos)
            {
                var activeOnUsVpos = await _repository.GetAll()
                    .AnyAsync(s => s.IsOnUsVpos == true && s.RecordStatus == RecordStatus.Active && s.Id != vpos.Id, cancellationToken: cancellationToken);
                if (activeOnUsVpos)
                {
                    throw new OnUsVposAlreadyActiveException();
                }
            }

            if (vpos.RecordStatus == RecordStatus.Passive)
            {
                var merchantVpos = await _merchantVposRepository
                                    .GetAll()
                                    .Where(b => b.VposId == vpos.Id && b.RecordStatus == RecordStatus.Active)
                                    .ToListAsync();
                if (merchantVpos.Any())
                {
                    throw new AlreadyInUseException(vpos.Name);
                }
                vpos.VposStatus = VposStatus.Inactive;  
            }

            if (vpos.RecordStatus == RecordStatus.Active)
            {
                vpos.VposStatus = VposStatus.Active;
            }

            await _repository.UpdateAsync(vpos);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateVpos",
                SourceApplication = "PF",
                Resource = "Vpos",
                Details = new Dictionary<string, string>
                {
                       {"Id", vpos.Id.ToString() },
                       {"Name", vpos.Name },
                       {"BankCode", vpos.AcquireBank?.BankCode.ToString() }
                }
            });

            return vposMap;
        }
        catch (Exception exception)
        {
            _logger.LogError($"VposPatchError : {exception}");
            throw;
        }
    }
}
