using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.Merchants.Command.PatchMerchantApiKey;

public class PatchMerchantApiKeyCommand : IRequest<MerchantApiKeyPatch>
{
    public Guid MerchantId { get; set; }
    public JsonPatchDocument<MerchantApiKeyPatch> MerchantApiKey { get; set; }
}

public class PatchMerchantApiKeyCommandHandler : IRequestHandler<PatchMerchantApiKeyCommand, MerchantApiKeyPatch>
{
    private readonly IGenericRepository<MerchantApiKey> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<MerchantApiKey> _logger;
    private readonly IAuditLogService _auditLogService;

    public PatchMerchantApiKeyCommandHandler(IGenericRepository<MerchantApiKey> repository, 
        IMapper mapper,
        ILogger<MerchantApiKey> logger,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
    }
    public async Task<MerchantApiKeyPatch> Handle(PatchMerchantApiKeyCommand request, CancellationToken cancellationToken)
    {
        var merchantApiKey = await _repository.GetAll().FirstOrDefaultAsync(b => b.MerchantId == request.MerchantId, cancellationToken);

        if (merchantApiKey is null)
        {
            throw new NotFoundException(nameof(MerchantApiKey), request.MerchantId);
        }

        try
        {
            var merchantApiKeyMap = _mapper.Map<MerchantApiKeyPatch>(merchantApiKey);

            request.MerchantApiKey.ApplyTo(merchantApiKeyMap);
            _mapper.Map(merchantApiKeyMap, merchantApiKey);

            await _repository.UpdateAsync(merchantApiKey);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {   
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateMerchantApiKey",
                SourceApplication = "PF",
                Resource = "MerchantApiKey",
                Details = new Dictionary<string, string>
                {
                       {"Id", merchantApiKey.Id.ToString() },
                       {"PublicKey", merchantApiKeyMap.PublicKey },
                       {"PrivateKeyEncrypted", merchantApiKeyMap.PrivateKeyEncrypted },
                }
            });

            return merchantApiKeyMap;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "MerchantApiKeyPatchError : {Exception}", exception);
            throw;
        }
    }
}
