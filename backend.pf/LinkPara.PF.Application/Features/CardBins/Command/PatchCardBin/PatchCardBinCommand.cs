using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.CardBins;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.CardBins.Command.PatchCardBin;

public class PatchCardBinCommand : IRequest<UpdateCardBinRequest>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateCardBinRequest> CardBin { get; set; }
}

public class PatchCardBinCommandHandler : IRequestHandler<PatchCardBinCommand, UpdateCardBinRequest>
{
    private readonly IGenericRepository<CardBin> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CardBin> _logger;
    private readonly IAuditLogService _auditLogService;

    public PatchCardBinCommandHandler(IGenericRepository<CardBin> repository, IMapper mapper, 
        ILogger<CardBin> logger,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService; 
    }
    public async Task<UpdateCardBinRequest> Handle(PatchCardBinCommand request, CancellationToken cancellationToken)
    {
        var cardBin = await _repository.GetAll().Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (cardBin is null)
        {
            throw new NotFoundException(nameof(CardBin), request.Id);
        }

        try
        {
            var cardBinMap = _mapper.Map<UpdateCardBinRequest>(cardBin);

            request.CardBin.ApplyTo(cardBinMap);
            _mapper.Map(cardBinMap, cardBin);

            await _repository.UpdateAsync(cardBin);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateCardBin",
                SourceApplication = "PF",
                Resource = "CardBin",
                Details = new Dictionary<string, string>
                {
                       {"BinNumber", cardBinMap.BinNumber.ToString() },
                       {"BankCode", cardBinMap.BankCode.ToString() },
                       {"CardType", cardBinMap.CardType.ToString() },
                       {"CardBrand", cardBinMap.CardBrand.ToString() },
                }
            });

            return cardBinMap;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "CardBinPatchError : {Exception}", exception);
            throw;
        }
    }
}
