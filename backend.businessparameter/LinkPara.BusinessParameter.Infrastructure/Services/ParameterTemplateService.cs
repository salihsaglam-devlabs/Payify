using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetAllParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Queries.GetParameterTemplateById;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.SharedModels.Pagination;
using LinkPara.Cache;
using AutoMapper.QueryableExtensions;
using LinkPara.MappingExtensions.Mapping;

namespace LinkPara.BusinessParameter.Infrastructure.Services;

public class ParameterTemplateService : IParameterTemplateService
{
    private readonly IGenericRepository<ParameterTemplate> _repository;
    private readonly IGenericRepository<ParameterGroup> _parameterGroup;
    private readonly IMapper _mapper;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ParameterTemplateService> _logger;
    private readonly ICacheService _cacheService;

    public ParameterTemplateService(IGenericRepository<ParameterTemplate> repository,
        IMapper mapper,
        IGenericRepository<ParameterGroup> parameterGroup,
        IApplicationUserService applicationUserService,
        IAuditLogService auditLogService,
        ILogger<ParameterTemplateService> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _parameterGroup = parameterGroup;
        _applicationUserService = applicationUserService;
        _auditLogService = auditLogService;
        _logger = logger;
        _cacheService= cacheService;    
    }
    public async Task<List<ParameterTemplateDto>> GetAllParameterTemplateByGroupCodeAsync(string groupCode)
    {

        var parametersTemplates = await _cacheService.GetOrCreateAsync<List<ParameterTemplate>>(string.Concat(groupCode, "Template")
                                                                , async () =>
                                                                {
                                                                    return await _repository.GetAll()
                                                                            .Where(b => b.GroupCode == groupCode).ToListAsync();
                                                                });

        return _mapper.Map<List<ParameterTemplateDto>>(parametersTemplates);
    }
    public async Task<ParameterTemplateDto> GetParameterTemplateAsync(string groupCode, string templateCode)
    {
        var parameterTemplate = await _repository.GetAll()
          .FirstOrDefaultAsync(b => b.GroupCode == groupCode
          && b.TemplateCode == templateCode);

        if (parameterTemplate is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), templateCode);
        }

        return _mapper.Map<ParameterTemplateDto>(parameterTemplate);
    }
    public async Task SaveAsync(SaveParameterTemplateCommand request)
    {
        var parameterTemplate = await _repository.GetAll().FirstOrDefaultAsync(
                   b => b.TemplateCode == request.TemplateCode && b.GroupCode == request.GroupCode
                   && b.RecordStatus == RecordStatus.Active);

        if (parameterTemplate is not null)
        {
            throw new DuplicateRecordException(nameof(ParameterTemplate), request.GroupCode);
        }

        var parameterGroup = await _parameterGroup.GetAll()
          .FirstOrDefaultAsync(b => b.GroupCode == request.GroupCode && b.RecordStatus == RecordStatus.Active);

        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), request.GroupCode);
        }

        parameterTemplate = _mapper.Map<ParameterTemplate>(request);

        parameterTemplate.CreatedBy = _applicationUserService.ApplicationUserId.ToString();

        await _repository.AddAsync(parameterTemplate);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveParameterTemplate",
                SourceApplication = "BusinessParameter",
                Resource = "ParameterTemplate",
                Details = new Dictionary<string, string>
                {
                        {"Id", parameterTemplate.Id.ToString() },
                        {"GroupCode", request.GroupCode },
                        {"TemplateCode", request.TemplateCode },
                        {"DataType", request.DataType.ToString() },
                }
            });

    }
    public async Task<ParameterTemplateDto> GetByIdAsync(GetParameterTemplateByIdQuery request)
    {
        var parameterTemplate = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.RecordStatus == RecordStatus.Active);

        if (parameterTemplate is null)
        {
            throw new NotFoundException(nameof(ParameterTemplate), request.Id);
        }

        return _mapper.Map<ParameterTemplateDto>(parameterTemplate);
    }
    public async Task DeleteAsync(DeleteParameterTemplateCommand command)
    {
        var parameterTemplate = await _repository.GetByIdAsync(command.Id);

        if (parameterTemplate is null)
        {
            throw new NotFoundException(nameof(ParameterTemplate), command.Id);
        }

        try
        {
            parameterTemplate.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(parameterTemplate);
        }
        catch (Exception exception)
        {
            _logger.LogError("ParameterTemplateDeleteError : {exception}", exception);
            throw;
        }
    }
    public async Task<ParameterTemplateDto> UpdateAsync(UpdateParameterTemplateCommand command)
    {
        var parameterTemplate = await _repository.GetAll().FirstOrDefaultAsync(
                     b => b.Id == command.Id && b.RecordStatus == RecordStatus.Active);

        if (parameterTemplate is null)
        {
            throw new NotFoundException(nameof(ParameterTemplate), command.Id);
        }

        var duplicateParameterTemplate = await _repository.GetAll().FirstOrDefaultAsync(
                                                                     b => b.Id != command.Id
                                                                     && b.TemplateCode == command.TemplateCode 
                                                                     && b.RecordStatus == RecordStatus.Active
                                                                     && b.GroupCode == command.GroupCode);

        if (duplicateParameterTemplate is not null)
        {
            throw new DuplicateRecordException(nameof(ParameterTemplate), command.TemplateCode);
        }

        var parameterGroup = await _parameterGroup.GetAll().FirstOrDefaultAsync(
                   b => b.GroupCode == command.GroupCode && b.RecordStatus == RecordStatus.Active);

        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), command.Id);
        }
        try
        {
            parameterTemplate = _mapper.Map(command, parameterTemplate);
            await _repository.UpdateAsync(parameterTemplate);
        }
        catch (Exception exception)
        {
            _logger.LogError("ParameterTemplateUpdateError : {exception}", exception);
            throw;
        }
        return _mapper.Map<ParameterTemplateDto>(parameterTemplate);
    }
    public async Task<PaginatedList<ParameterTemplateDto>> GetAllParameterTemplateAsync(GetAllParameterTemplateQuery request)
    {
        var query = _repository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active);

        if (!string.IsNullOrEmpty(request.GroupCode))
        {
            query = query.Where(x => x.GroupCode.Contains(request.GroupCode));
        }

        return await query.PaginatedListWithMappingAsync<ParameterTemplate,ParameterTemplateDto>(_mapper, request.Page,
            request.Size, request.OrderBy, request.SortBy);
    }

}
