using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantCategoryCodes;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.PatchMcc;

public class PatchMccCommand : IRequest<UpdateMccRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateMccRequest> Mcc { get; set; }
}

public class PatchMccCommandHandler : IRequestHandler<PatchMccCommand, UpdateMccRequest>
{
    private readonly IGenericRepository<Mcc> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<Mcc> _logger;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IAuditLogService _auditLogService;

    public PatchMccCommandHandler(IGenericRepository<Mcc> repository, IMapper mapper,
        ILogger<Mcc> logger, IGenericRepository<Merchant> merchantRepository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _merchantRepository = merchantRepository;
        _auditLogService = auditLogService;
    }
    public async Task<UpdateMccRequest> Handle(PatchMccCommand request, CancellationToken cancellationToken)
    {
        var mcc = await _repository.GetAll()
                           .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (mcc is null)
        {
            throw new NotFoundException(nameof(Mcc), request.Id);
        }

        try
        {
            var mccMap = _mapper.Map<UpdateMccRequest>(mcc);

            request.Mcc.ApplyTo(mccMap);
            _mapper.Map(mccMap, mcc);

            if (mcc.RecordStatus == RecordStatus.Passive)
            {
                var merchants = await _merchantRepository.GetAll()
                    .Where(b => b.MccCode == mcc.Code
                    && b.RecordStatus == RecordStatus.Active).ToListAsync(cancellationToken);

                if (merchants.Any())
                {
                    throw new AlreadyInUseException(nameof(Merchant));
                }
            }

            await _repository.UpdateAsync(mcc);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateMcc",
                SourceApplication = "PF",
                Resource = "Mcc",
                Details = new Dictionary<string, string>
                {
                        {"Id", mcc.Id.ToString() },
                        {"Name", mcc.Name},
                        {"Code", mcc.Code}
                }
            });

            return mccMap;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "MccPatchError : {Exception}", exception);
            throw;
        }
    }
}
