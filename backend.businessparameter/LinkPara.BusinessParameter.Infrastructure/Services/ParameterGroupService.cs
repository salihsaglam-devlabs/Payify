using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.DeleteParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.SaveParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetAllParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetParameterGroupById;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.BusinessParameter.Infrastructure.Services;

public class ParameterGroupService : IParameterGroupService
{
    private readonly ILogger<ParameterGroupService> _logger;
    private readonly IGenericRepository<ParameterGroup> _repository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public ParameterGroupService(IGenericRepository<ParameterGroup> repository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        ILogger<ParameterGroupService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    public async Task SaveAsync(SaveParameterGroupCommand request)
    {
        var parameterGroup = await _repository.GetAll()
                              .FirstOrDefaultAsync(b => b.GroupCode == request.GroupCode);

        if (parameterGroup is not null)
        {
            throw new DuplicateRecordException(nameof(ParameterGroup), request.GroupCode);
        }

        parameterGroup = _mapper.Map<ParameterGroup>(request);
        parameterGroup.CreatedBy = _contextProvider.CurrentContext.UserId;

        await _repository.AddAsync(parameterGroup);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveParameterGroup",
                SourceApplication = "BusinessParameter",
                Resource = "ParameterGroup",
                Details = new Dictionary<string, string>
                {
                        {"Id", parameterGroup.Id.ToString() },
                        {"GroupCode", request.GroupCode },
                        {"Explanation", request.Explanation },
                }
            });

    }
    public async Task<PaginatedList<ParameterGroupDto>> GetAllParameterGroupAsync(GetAllParameterGroupQuery request)
    {
        var query = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.GroupCode))
        {
            query = query.Where(x => x.GroupCode.Contains(request.GroupCode));
        }
        if (request.RecordStatus.HasValue)
        {
            query = query.Where(x => x.RecordStatus == request.RecordStatus);
        }

        return await query.PaginatedListWithMappingAsync<ParameterGroup,ParameterGroupDto>(_mapper, request.Page,
            request.Size,request.OrderBy, request.SortBy);
    }
    public async Task<ParameterGroupDto> GetByIdAsync(GetParameterGroupByIdQuery request)
    {
        var parameterGroup = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), request.Id);
        }

        return _mapper.Map<ParameterGroupDto>(parameterGroup);
    }
    public async Task DeleteAsync(DeleteParameterGroupCommand command)
    {
        var parameterGroup = await _repository.GetByIdAsync(command.Id);

        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), command.Id);
        }

        try
        {
            parameterGroup.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(parameterGroup);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ParameterGroupDeleteError : {exception}");
            throw;
        }
    }
    public async Task<ParameterGroupDto> UpdateAsync(UpdateParameterGroupCommand command)
    {
        var parameterGroup = await _repository.GetAll().FirstOrDefaultAsync(
                     b => b.Id == command.Id);
        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), command.Id);
        }
        var duplicateParameterGroup = await _repository.GetAll().FirstOrDefaultAsync(
                   b => b.Id != command.Id && b.GroupCode == command.GroupCode && b.RecordStatus==RecordStatus.Active);

        if (duplicateParameterGroup is not null)
        {
            throw new DuplicateRecordException(nameof(ParameterGroup), command.GroupCode);
        }

        try
        {
            parameterGroup= _mapper.Map(command, parameterGroup);
            await _repository.UpdateAsync(parameterGroup);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ParameterGroupUpdateError : {exception}");
            throw;
        }

        return _mapper.Map<ParameterGroupDto>(parameterGroup);
    }
}
