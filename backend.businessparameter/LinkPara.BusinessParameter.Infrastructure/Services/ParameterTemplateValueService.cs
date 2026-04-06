using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.DeleteParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.SaveParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.UpdateParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetAllParameterTemplateValue;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetParameterTemplateValueById;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.BusinessParameter.Infrastructure.Services;
public class ParameterTemplateValueService : IParameterTemplateValueService
{
    private readonly IGenericRepository<ParameterTemplateValue> _repository;
    private readonly IGenericRepository<ParameterGroup> _parameterGroupRepository;
    private readonly IGenericRepository<Parameter> _parameterRepository;
    private readonly IGenericRepository<ParameterTemplate> _parameterTemplateRepository;
    private readonly IMapper _mapper;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ILogger<ParameterTemplateService> _logger;
    private readonly IAuditLogService _auditLogService;

    public ParameterTemplateValueService(IGenericRepository<ParameterTemplateValue> repository,
        IMapper mapper,
        IGenericRepository<ParameterGroup> parameterGroupRepository,
        IApplicationUserService applicationUserService,
        IGenericRepository<Parameter> parameterRepository,
        ILogger<ParameterTemplateService> logger,
        IAuditLogService auditLogService,
        IGenericRepository<ParameterTemplate> parameterTemplateRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _parameterGroupRepository = parameterGroupRepository;
        _applicationUserService = applicationUserService;
        _parameterRepository = parameterRepository;
        _logger = logger;
        _auditLogService = auditLogService;
        _parameterTemplateRepository = parameterTemplateRepository;
    }
    public async Task<ParameterTemplateValueDto> GetParameterTemplateValueAsync(string groupCode, string parameterCode, string templateCode)
    {
        var parameterTemplateValue = await _repository.GetAll().Include(x=>x.Parameter)
              .FirstOrDefaultAsync(b => b.GroupCode == groupCode
              && b.ParameterCode == parameterCode
              && b.TemplateCode == templateCode);
        if (parameterTemplateValue is null)
        {
            throw new NotFoundException(nameof(ParameterTemplateValue), templateCode);
        }
        return _mapper.Map<ParameterTemplateValueDto>(parameterTemplateValue);
    }
    public async Task<List<ParameterTemplateValueDto>> GetAllParameterTemplateValuesByGroupCodeAsync(string groupCode, string parameterCode)
    {
        var parameterTemplateValues = _repository.GetAll();
        if (!string.IsNullOrEmpty(parameterCode))
        {
            parameterTemplateValues = parameterTemplateValues.Include(x => x.Parameter)
                .Where(b => b.ParameterCode == parameterCode);
        }

        var list = await parameterTemplateValues.Include(x => x.Parameter)
            .Where(b => b.GroupCode == groupCode).ToListAsync();
        return _mapper.Map<List<ParameterTemplateValueDto>>(list);
    }
    public async Task<PaginatedList<ParameterTemplateValueDto>> GetAllParameterTemplateValuesAsync(GetAllParameterTemplateValueQuery request)
    {
        var parameterTemplateValues = _repository.GetAll().Include(b => b.Parameter).Where(x => x.RecordStatus == RecordStatus.Active);

        return await parameterTemplateValues
               .PaginatedListWithMappingAsync<ParameterTemplateValue,ParameterTemplateValueDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
    public async Task SaveAsync(SaveParameterTemplateValueCommand request)
    {
        var activeParametre = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.GroupCode == request.GroupCode 
            && b.ParameterCode == request.ParameterCode 
            && b.TemplateCode == request.TemplateCode
            && b.RecordStatus == RecordStatus.Active);

        if (activeParametre is not null)
        {
            throw new DuplicateRecordException(nameof(ParameterTemplateValue));
        }

        var parameterGroup = await _parameterGroupRepository.GetAll()
          .FirstOrDefaultAsync(b => b.GroupCode == request.GroupCode && b.RecordStatus == RecordStatus.Active);
        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), request.GroupCode);
        }
        var parameterTemplate = await _parameterTemplateRepository.GetAll()
       .FirstOrDefaultAsync(b => b.TemplateCode == request.TemplateCode && b.RecordStatus == RecordStatus.Active);

        if (parameterTemplate is null)
        {
            throw new NotFoundException(nameof(ParameterTemplate), request.TemplateCode);
        }
        var parametre = new Parameter
        {
            GroupCode = request.GroupCode,
            ParameterValue = request.ParameterValue,
            ParameterCode = request.ParameterCode,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        };
        await _parameterRepository.AddAsync(parametre);

        var parameterTemplateValue = _mapper.Map<ParameterTemplateValue>(request);
        parameterTemplateValue.CreatedBy = _applicationUserService.ApplicationUserId.ToString();

        await _repository.AddAsync(parameterTemplateValue);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveParameterTemplateValue",
                SourceApplication = "BusinessParameter",
                Resource = "ParameterTemplateValue",
                Details = new Dictionary<string, string>
                {
                      {"Id", parameterTemplateValue.Id.ToString() },
                      {"GroupCode", request.GroupCode },
                      {"ParameterCode", request.ParameterCode },
                      {"TemplateCode", request.TemplateCode },
                }
            });

    }
    public async Task<ParameterTemplateValueDto> GetByIdAsync(GetParameterTemplateValueByIdQuery request)
    {
        var parameterTemplateValue = await _repository.GetAll().Include(b => b.Parameter)
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.RecordStatus == RecordStatus.Active);
        if (parameterTemplateValue is null)
        {
            throw new NotFoundException(nameof(ParameterTemplateValue), request.Id);
        }
        return _mapper.Map<ParameterTemplateValueDto>(parameterTemplateValue);
    }
    public async Task DeleteAsync(DeleteParameterTemplateValueCommand command)
    {
        var parameterTemplateValue = await _repository.GetAll().Include(x => x.Parameter).FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active);

        if (parameterTemplateValue is null)
        {
            throw new NotFoundException(nameof(ParameterTemplateValue), command.Id);
        }

        try
        {
            parameterTemplateValue.RecordStatus = RecordStatus.Passive;
            await _repository.UpdateAsync(parameterTemplateValue);

            var parameter = await _parameterRepository.GetByIdAsync(parameterTemplateValue.Parameter?.Id);
            if (parameter != null)
            {
                parameter.RecordStatus = RecordStatus.Passive;
                await _parameterRepository.UpdateAsync(parameter);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ParameterTemplateValueDeleteError : {exception}", exception);
            throw;
        }
    }
    public async Task<ParameterTemplateValueDto> UpdateAsync(UpdateParameterTemplateValueCommand command)
    {
        var parameterTemplateValue = await _repository.GetAll().Include(x => x.Parameter).FirstOrDefaultAsync(
                     b => b.Id == command.Id && b.RecordStatus == RecordStatus.Active);
        if (parameterTemplateValue is null)
        {
            throw new NotFoundException(nameof(ParameterTemplateValue), command.Id);
        }
        try
        {
            parameterTemplateValue = _mapper.Map(command, parameterTemplateValue);
            if (parameterTemplateValue.Parameter.ParameterCode != command.ParameterCode)
            {
                var parametre = new Parameter
                {
                    GroupCode = command.GroupCode,
                    ParameterValue = command.ParameterValue,
                    ParameterCode = command.ParameterCode,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                };
                await _parameterRepository.AddAsync(parametre);
                parameterTemplateValue.ParameterCode = parametre.ParameterCode;
            }

            await _repository.UpdateAsync(parameterTemplateValue);
        }
        catch (Exception exception)
        {
            _logger.LogError("ParameterTemplateValueUpdateError : {exception}", exception);
            throw;
        }
        return _mapper.Map<ParameterTemplateValueDto>(parameterTemplateValue);
    }
}
