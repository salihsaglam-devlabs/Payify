using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.SaveMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.UpdateMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetAllMerchantPreApplication;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantPreApplicationService : IMerchantPreApplicationService
{
    private readonly IGenericRepository<MerchantPreApplication> _merchantPreApplicationRepository;
    private readonly IGenericRepository<MerchantPreApplicationHistory> _merchantPreApplicationHistoryRepository;
    private readonly IMapper _mapper;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ILogger<MerchantPreApplication> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IStringLocalizer _notificationLocalizer;
    private readonly IBus _bus;
    public MerchantPreApplicationService(
        IGenericRepository<MerchantPreApplication> merchantPreApplicationRepository, 
        IGenericRepository<MerchantPreApplicationHistory> merchantPreApplicationHistoryRepository,
        IMapper mapper,
        IContextProvider contextProvider,
        IApplicationUserService applicationUserService,
        ILogger<MerchantPreApplication> logger,
        IAuditLogService auditLogService,
        IStringLocalizerFactory factory,
        IBus bus)
    {
        _merchantPreApplicationRepository = merchantPreApplicationRepository;
        _merchantPreApplicationHistoryRepository = merchantPreApplicationHistoryRepository;
        _mapper = mapper;
        _contextProvider = contextProvider;
        _applicationUserService = applicationUserService;
        _logger = logger;
        _auditLogService = auditLogService;
        _notificationLocalizer = factory.Create("Notifications", "LinkPara.PF.API");
        _bus = bus;
    }

    public async Task<MerchantPreApplicationCreateResponse> SaveAsync(SaveMerchantPreApplicationCommand request)
    {
        var activeApplication = await _merchantPreApplicationRepository.GetAll()
            .Where(a => a.PhoneNumber == request.PhoneNumber && a.Email == request.Email)
            .FirstOrDefaultAsync();

        if (activeApplication is not null)
        {
            throw new DuplicateRecordException();
        }
        
        try
        {
            var newApplication = new MerchantPreApplication
            {
                FullName = request.Name + " " + request.Surname,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                ProductTypes = request.ProductTypes,
                MonthlyTurnover = request.MonthlyTurnover,
                Website = request.Website,
                ConsentConfirmation = request.ConsentConfirmation,
                KvkkConfirmation = request.KvkkConfirmation,
                RecordStatus = RecordStatus.Active,
                CreateDate = DateTime.Now,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                ApplicationStatus = ApplicationStatus.New
            };

            var createdApplication = await _merchantPreApplicationRepository.AddAsync(newApplication);

            await SendApplicationReceivedEmail(createdApplication);
   
            return new MerchantPreApplicationCreateResponse
            {
                Id = createdApplication.Id,
                IsSuccess = true
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "SaveMerchantPreApplicationError : {Exception}", exception);
            throw new Exception("SaveMerchantPreApplicationError", exception);
        }
    }

    public async Task<MerchantPreApplicationDto> GetPosApplicationByIdAsync(Guid id)
    {
        var application = await _merchantPreApplicationRepository.GetAll()
            .Include(a => a.ApplicationHistories)
                .Where(a => a.Id == id).FirstOrDefaultAsync();
        
        return _mapper.Map<MerchantPreApplicationDto>(application);
    }

    public async Task<PaginatedList<MerchantPreApplicationDto>> GetFilterAsync(GetAllMerchantPreApplicationQuery request)
    {
        var applications = _merchantPreApplicationRepository.GetAll().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            applications = applications.Where(a => a.PhoneNumber.Contains(request.PhoneNumber));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            applications = applications.Where(a => a.Email.Contains(request.Email));
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            applications = applications.Where(a => a.FullName.Contains(request.FullName));
        }
        
        if (!string.IsNullOrWhiteSpace(request.ResponsiblePerson))
        {
            applications = applications.Where(a => a.ResponsiblePerson.Contains(request.ResponsiblePerson));
        }
        
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            applications = applications.Where(a => a.Website.Contains(request.Website));
        }

        if (request.CreateDateStart is not null)
        {
            applications = applications.Where(a => a.CreateDate >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            applications = applications.Where(a => a.CreateDate <= request.CreateDateEnd);
        }

        if (request.ProductType is not null)
        {
            var type = request.ProductType.ToString();
            applications = applications
                .Where(b => ((string)(object)b.ProductTypes).Contains(type));
        }

        if (request.ApplicationStatus is not null)
        {
            applications = applications.Where(a => a.ApplicationStatus == request.ApplicationStatus);
        }

        if (request.MonthlyTurnover is not null)
        {
            applications = applications.Where(a => a.MonthlyTurnover == request.MonthlyTurnover);
        }

        return await applications.Include(a => a.ApplicationHistories)
            .PaginatedListWithMappingAsync<MerchantPreApplication, MerchantPreApplicationDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task UpdatePosApplicationAsync(UpdateMerchantPreApplicationCommand request)
    {
        var application = await _merchantPreApplicationRepository.GetAll()
            .Include(a => a.ApplicationHistories
                .OrderBy(h => h.OperationDate))
            .FirstOrDefaultAsync(a => a.Id == request.Id);
        
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (application is null)
        {
            throw new NotFoundException(nameof(application));
        }
        
        application.FullName = request.FullName;
        application.Email = request.Email;
        application.PhoneNumber = request.PhoneNumber;
        application.ApplicationStatus = request.ApplicationStatus;
        application.ProductTypes = request.ProductTypes;
        application.MonthlyTurnover = request.MonthlyTurnover;
        application.Website = request.Website;
        application.UpdateDate = DateTime.Now;

        try
        {
            if (request.ApplicationHistories.Count != application.ApplicationHistories.Count)
            {
                foreach (var history in request.ApplicationHistories)
                {
                    if (history.Id == Guid.Empty)
                    {
                        var newHistory = new MerchantPreApplicationHistory
                        {
                            MerchantPreApplicationId = history.MerchantPreApplicationId,
                            UserId = history.UserId,
                            UserName = history.UserName,
                            OperationType = history.OperationType,
                            OperationDate = DateTime.Now,
                            OperationNote = history.OperationNote,
                            CreateDate = DateTime.Now,
                            CreatedBy = parseUserId.ToString(),
                        };
                        var applicationHistory = await _merchantPreApplicationHistoryRepository.AddAsync(newHistory);

                        if (applicationHistory.OperationType == ApplicationOperationType.Evaluate)
                        {
                            application.ApplicationStatus = ApplicationStatus.InProgress;
                            application.ResponsiblePerson = request.ResponsiblePerson;
                        }
                        else if (applicationHistory.OperationType == ApplicationOperationType.Approve)
                        {
                            application.ApplicationStatus = ApplicationStatus.Approved;
                        }
                        else if (applicationHistory.OperationType == ApplicationOperationType.Reject)
                        {
                            application.ApplicationStatus = ApplicationStatus.Rejected;
                        }
                        
                        application.ApplicationHistories.Add(applicationHistory);
                    }
                }
            }

            await _merchantPreApplicationRepository.UpdateAsync(application);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "MerchantPreApplicationUpdateError : {Exception}", exception);
        }
        
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateMerchantPreApplication",
                SourceApplication = "PF",
                Resource = "MerchantPreApplication",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                    {"Id", request.Id.ToString()},
                    {"Name", request.FullName},
                    {"Pos Product Type", request.ProductTypes.ToString()},
                    {"Application Status", request.ApplicationStatus.ToString()},
                    {"Monthly Turnover", request.MonthlyTurnover.ToString()},
                }
            });
    }

    public async Task DeleteAsync(Guid id)
    {
        var application = await _merchantPreApplicationRepository.GetByIdAsync(id);
        
        if (application is null)
        {
            throw new NotFoundException(nameof(MerchantPreApplication), id);
        }
        
        application.RecordStatus = RecordStatus.Passive;

        try
        {
            foreach (var history in application.ApplicationHistories)
            {
                history.RecordStatus = RecordStatus.Passive;

                await _merchantPreApplicationHistoryRepository.UpdateAsync(history);
            }

            await _merchantPreApplicationRepository.UpdateAsync(application);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "PendingPosApplicationDeleteError : {Exception}", exception);
        }
    }

    private async Task SendApplicationReceivedEmail(MerchantPreApplication application)
    {
        await _bus.Publish(new SharedModels.Notification.NotificationModels.PF.MerchantPreApplication
        {
            FullName = application.FullName,
            PhoneNumber = application.PhoneNumber,
            Email = application.Email,
            ProductType = GetProductTypeString(application.ProductTypes),
            MonthlyTurnover = _notificationLocalizer.GetString(application.MonthlyTurnover.ToString()).Value,
            Website = application.Website,
            CreateDate = application.CreateDate.ToString("dd.MM.yyyy HH:mm:ss")
        });
    }
    
    public string GetProductTypeString(PosProductType productTypes)
    {
        if (productTypes == PosProductType.Unknown)
            return "[ Unknown ]";

        var names = Enum.GetValues(typeof(PosProductType));
        var selectedTypes = new List<string>();

        foreach (PosProductType type in names)
        {
            if (type != PosProductType.Unknown && productTypes.HasFlag(type))
            {
                selectedTypes.Add(_notificationLocalizer.GetString(type.ToString()).Value);
            }
        }

        return $"[ {string.Join(" | ", selectedTypes)} ]";
    }
}