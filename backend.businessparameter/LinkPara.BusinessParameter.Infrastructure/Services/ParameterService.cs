using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.Parameters;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.DeleteParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.SaveParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.UpdateParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetAllParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetParameterById;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.Cache;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.BusinessParameter.Infrastructure.Services;

public class ParameterService : IParameterService
{
    private readonly IGenericRepository<Parameter> _parameterRepository;
    private readonly IGenericRepository<ParameterGroup> _parameterGroupRepository;
    private readonly IGenericRepository<ParameterTemplate> _parameterTemplateRepository;
    private readonly IGenericRepository<ParameterTemplateValue> _parameterTemplateValueRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ParameterTemplateService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly ICacheService _cacheService;

    public ParameterService(IGenericRepository<Parameter> parameterRepository,
        IMapper mapper,
        IApplicationUserService applicationUserService,
        IGenericRepository<ParameterGroup> parameterGroupRepository,
        IGenericRepository<ParameterTemplate> parameterTemplateRepository,
        IGenericRepository<ParameterTemplateValue> parameterTemplateValueRepository,
        IAuditLogService auditLogService,
        ILogger<ParameterTemplateService> logger,
        ICacheService cacheService)
    {
        _parameterRepository = parameterRepository;
        _mapper = mapper;
        _applicationUserService = applicationUserService;
        _parameterTemplateRepository = parameterTemplateRepository;
        _parameterGroupRepository = parameterGroupRepository;
        _parameterTemplateValueRepository = parameterTemplateValueRepository;
        _logger = logger;
        _auditLogService = auditLogService;
        _cacheService = cacheService;
    }
    public async Task<ParameterDto> GetParameterAsync(string groupCode, string parameterCode)
    {
        var parameter = await _parameterRepository.GetAll()
            .FirstOrDefaultAsync(b => b.GroupCode == groupCode
            && b.ParameterCode == parameterCode);

        if (parameter is null)
        {
            throw new NotFoundException(nameof(Parameter), parameterCode);
        }

        return _mapper.Map<ParameterDto>(parameter);
    }
    public async Task<List<ParameterDto>> GetParametersAsync(string groupCode)
    {
        var parameters = await _cacheService.GetOrCreateAsync<List<Parameter>>(groupCode,
            async () =>
            {
                return await _parameterRepository.GetAll()
               .Where(b => b.GroupCode == groupCode)
               .OrderBy(b => b.ParameterValue).ToListAsync();
            });

        return _mapper.Map<List<ParameterDto>>(parameters);
    }
    public async Task SaveAsync(SaveParameterCommand request)
    {
        var parameterGroup = await _parameterGroupRepository.GetAll()
          .FirstOrDefaultAsync(b => b.GroupCode == request.GroupCode && b.RecordStatus == RecordStatus.Active);
        if (parameterGroup is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), request.GroupCode);
        }

        if (!String.IsNullOrEmpty(request.ParameterCode))
        {
            var findparameter = await _parameterRepository.GetAll()
                 .FirstOrDefaultAsync(b => b.ParameterCode == request.ParameterCode &&
                 b.RecordStatus == RecordStatus.Active && b.GroupCode == request.GroupCode);

            if (findparameter is not null)
            {
                throw new DuplicateRecordException(nameof(Parameter), request.ParameterCode);
            }
        }

        var parametre = new Parameter
        {
            GroupCode = request.GroupCode,
            ParameterValue = request.ParameterValue,
            ParameterCode = request.ParameterCode,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        };
        await _parameterRepository.AddAsync(parametre);

        foreach (var item in request.ParameterTemplateValueList)
        {
            var parameterTemplate = await _parameterTemplateRepository.GetAll()
                .FirstOrDefaultAsync(b => b.TemplateCode == item.TemplateCode && b.RecordStatus == RecordStatus.Active);
            if (parameterTemplate is null)
            {
                throw new NotFoundException(nameof(ParameterTemplate), item.TemplateCode);
            }
            var parameterTemplateValue = new ParameterTemplateValue();
            parameterTemplateValue.ParameterCode = request.ParameterCode;
            parameterTemplateValue.GroupCode = request.GroupCode;
            parameterTemplateValue.TemplateCode = item.TemplateCode;
            parameterTemplateValue.TemplateValue = item.TemplateValue;
            parameterTemplateValue.CreatedBy = _applicationUserService.ApplicationUserId.ToString();

            await _parameterTemplateValueRepository.AddAsync(parameterTemplateValue);
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveParameter",
                SourceApplication = "BusinessParameter",
                Resource = "Parameter",
                Details = new Dictionary<string, string>
                {
                       {"Id", parametre.Id.ToString() },
                       {"GroupCode", request.GroupCode },
                       {"ParameterCode", request.ParameterCode },
                       {"ParameterValue", request.ParameterValue },
                }
            });

    }
    public async Task<PaginatedList<ParameterDto>> GetAllParameterAsync(GetAllParameterQuery request)
    {
        var query = _parameterRepository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active);

        if (!string.IsNullOrEmpty(request.GroupCode))
        {
            query = query.Where(x => x.GroupCode.Contains(request.GroupCode));
        }

        return await query.PaginatedListWithMappingAsync<Parameter,ParameterDto>(_mapper, request.Page,
            request.Size, request.OrderBy, request.SortBy);
    }
    public async Task<ParameterDto> GetByIdAsync(GetParameterByIdQuery request)
    {
        var parameter = await _parameterRepository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.RecordStatus == RecordStatus.Active);
        if (parameter is null)
        {
            throw new NotFoundException(nameof(Parameter), request.Id);
        }
        var resModel = _mapper.Map<ParameterDto>(parameter);

        var parameterTemplateValueList = _parameterTemplateValueRepository.GetAll()
            .Where(x => x.Parameter.Id == parameter.Id && x.RecordStatus == RecordStatus.Active);

        resModel.ParameterTemplateValueList = new List<ParameterTemplateValueResponse>();

        foreach (var item in parameterTemplateValueList)
        {
            var parameterTemplateValue = _mapper.Map<ParameterTemplateValueResponse>(item);
            resModel.ParameterTemplateValueList.Add(parameterTemplateValue);
        }

        return resModel;
    }
    public async Task DeleteAsync(DeleteParameterCommand command)
    {
        var parameter = await _parameterRepository.GetByIdAsync(command.Id);

        if (parameter is null)
        {
            throw new NotFoundException(nameof(Parameter), command.Id);
        }
        try
        {
            parameter.RecordStatus = RecordStatus.Passive;
            await _parameterRepository.UpdateAsync(parameter);

            var parameterTemplateValues = await _parameterTemplateValueRepository.GetAll()
                .Where(x => x.Parameter.Id == parameter.Id && x.RecordStatus == RecordStatus.Active).ToListAsync();

            foreach (var item in parameterTemplateValues)
            {
                item.RecordStatus = RecordStatus.Passive;
                await _parameterTemplateValueRepository.UpdateAsync(item);
            }           
        }
        catch (Exception exception)
        {
            _logger.LogError("ParameterDeleteError : {exception}", exception);
            throw;
        }
    }
    public async Task<ParameterDto> UpdateAsync(UpdateParameterCommand command)
    {
        var parameter = await _parameterRepository.GetAll().FirstOrDefaultAsync(
                     b => b.Id == command.Id && b.RecordStatus == RecordStatus.Active);
        if (parameter is null)
        {
            throw new NotFoundException(nameof(Parameter), command.Id);
        }

        try
        {
            parameter = _mapper.Map(command, parameter);

            await _parameterRepository.UpdateAsync(parameter);

            foreach (var item in command.ParameterTemplateValueList)
            {
                var parameterTemplateValue = await _parameterTemplateValueRepository.GetAll()
                    .FirstOrDefaultAsync(b => b.Id == item.Id && b.RecordStatus == RecordStatus.Active);

                if (parameterTemplateValue is null)
                {
                    throw new NotFoundException(nameof(ParameterTemplateValue), command.Id);
                }

                parameterTemplateValue.TemplateValue = item.TemplateValue;
                await _parameterTemplateValueRepository.UpdateAsync(parameterTemplateValue);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ParameterUpdateError : {exception}", exception);
            throw;
        }
        return _mapper.Map<ParameterDto>(parameter);
    }
}
