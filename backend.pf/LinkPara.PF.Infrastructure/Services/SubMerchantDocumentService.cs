using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.SaveSubMerchantDocument;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.UpdateSubMerchantDocument;
using LinkPara.PF.Application.Features.SubMerchantDocuments.Queries.GetAllSubMerchantDocuments;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class SubMerchantDocumentService : ISubMerchantDocumentService
{
    private readonly IGenericRepository<SubMerchantDocument> _subMerchantDocumentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubMerchantDocumentService> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;

    public SubMerchantDocumentService(
        IGenericRepository<SubMerchantDocument> subMerchantDocumentRepository,
        IMapper mapper,
        ILogger<SubMerchantDocumentService> logger,
        IContextProvider contextProvider,
        IAuditLogService auditLogService)
    {
        _subMerchantDocumentRepository = subMerchantDocumentRepository;
        _mapper = mapper;
        _logger = logger;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
    }

    public async Task DeleteAsync(Guid documentId)
    {
        var subMerchantDocument = await _subMerchantDocumentRepository.GetAll()
            .FirstOrDefaultAsync(s => s.DocumentId == documentId);

        if (subMerchantDocument is null)
        {
            throw new NotFoundException(nameof(SubMerchantDocument), documentId);
        }

        await _subMerchantDocumentRepository.DeleteAsync(subMerchantDocument);
    }

    public async Task SaveAsync(SaveSubMerchantDocumentCommand request)
    {
        var activeSubMerchantDocument = await _subMerchantDocumentRepository.GetAll()
            .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId && d.SubMerchantId == request.SubMerchantId);

        if (activeSubMerchantDocument is not null)
        {
            throw new DuplicateRecordException(nameof(SubMerchantDocument), request);
        }

        try
        {
            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
        
            var newSubMerhantDocument = _mapper.Map<SubMerchantDocument>(request);
            newSubMerhantDocument.CreateDate = DateTime.Now;
            newSubMerhantDocument.CreatedBy = parseUserId.ToString();
            newSubMerhantDocument.RecordStatus = RecordStatus.Active;
        
            await _subMerchantDocumentRepository.AddAsync(newSubMerhantDocument);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveSubMerchantDocument",
                    SourceApplication = "PF",
                    Resource = "SubMerchantDocument",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        { "DocumentName", request.DocumentName },
                        { "SubMerchantNumber", request.SubMerchantId.ToString() }
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "SubMerchantDocumentCreateError : {exception}", exception);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateSubMerchantDocumentCommand request)
    {
        var subMerhantDocument = await _subMerchantDocumentRepository.GetByIdAsync(request.SubMerchantDocument.Id);

        if (subMerhantDocument is null)
        {
            throw new NotFoundException(nameof(SubMerchantDocument), request.SubMerchantDocument.Id);
        }
        
        subMerhantDocument.DocumentId = request.SubMerchantDocument.DocumentId;
        subMerhantDocument.DocumentName = request.SubMerchantDocument.DocumentName;
        subMerhantDocument.DocumentTypeId = request.SubMerchantDocument.DocumentTypeId;
        subMerhantDocument.SubMerchantId = request.SubMerchantDocument.SubMerchantId;
        subMerhantDocument.UpdateDate = DateTime.Now;
        
        await _subMerchantDocumentRepository.UpdateAsync(subMerhantDocument);
    }

    public async Task<SubMerchantDocumentDto> GetByIdAsync(Guid documentId)
    {
        var subMerchantDocument = await _subMerchantDocumentRepository.GetByIdAsync(documentId);

        if (subMerchantDocument is null)
        {
            throw new NotFoundException(nameof(SubMerchantDocument), documentId);
        }

        return _mapper.Map<SubMerchantDocumentDto>(subMerchantDocument);
    }

    public async Task<PaginatedList<SubMerchantDocumentDto>> GetListAsync(GetAllSubMerchantDocumentsQuery request)
    {
        var queryResponse = _subMerchantDocumentRepository.GetAll().AsQueryable();

        if (request.DocumentId is not null)
        {
            queryResponse = queryResponse.Where(d => d.Id == request.DocumentId);
        }

        if (!string.IsNullOrEmpty(request.DocumentName))
        {
            queryResponse = queryResponse.Where(b => b.DocumentName.Contains(request.DocumentName));
        }

        if (request.SubMerchantId is not null)
        {
            queryResponse = queryResponse.Where(d => d.SubMerchantId == request.SubMerchantId);
        }

        if (request.DocumentTypeId is not null)
        {
            queryResponse = queryResponse.Where(d => d.DocumentTypeId == request.DocumentTypeId);
        }

        return await queryResponse
            .PaginatedListWithMappingAsync<SubMerchantDocument, SubMerchantDocumentDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
