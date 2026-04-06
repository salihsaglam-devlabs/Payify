using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles.Command.DeletePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.SavePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Queries.GetPricingProfileById;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LinkPara.PF.Infrastructure.Services;

public class PricingProfileService : IPricingProfileService
{
    private readonly ILogger<PricingProfileService> _logger;
    private readonly IGenericRepository<PricingProfile> _repository;
    private readonly IGenericRepository<PricingProfileItem> _itemRepository;
    private readonly IGenericRepository<PricingProfileInstallment> _itemInstallmentRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly ISecureRandomGenerator _randomGenerator;

    public PricingProfileService(ILogger<PricingProfileService> logger,
        IGenericRepository<PricingProfile> repository,
        IGenericRepository<PricingProfileItem> ItemRepository,
        IGenericRepository<Merchant> merchantRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        ISecureRandomGenerator randomGenerator, 
        IGenericRepository<PricingProfileInstallment> itemInstallmentRepository)
    {
        _logger = logger;
        _repository = repository;
        _itemRepository = ItemRepository;
        _merchantRepository = merchantRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _randomGenerator = randomGenerator;
        _itemInstallmentRepository = itemInstallmentRepository;
    }

    public async Task DeleteAsync(DeletePricingProfileCommand request)
    {
        var pricingProfile = await _repository.GetByIdAsync(request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingProfile), request.Id);
        }

        try
        {
            var merchants = await _merchantRepository.GetAll()
                .Where(b => b.PricingProfileNumber == pricingProfile.PricingProfileNumber 
                            && b.RecordStatus == RecordStatus.Active).ToListAsync();

            if (merchants.Any())
            {
                throw new AlreadyInUseException(nameof(PricingProfile));
            }
            
            pricingProfile.ProfileStatus = ProfileStatus.Deleted;
            pricingProfile.RecordStatus = SharedModels.Persistence.RecordStatus.Passive;

            await _repository.UpdateAsync(pricingProfile);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeletePricingProfile",
                    SourceApplication = "PF",
                    Resource = "PricingProfile",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString()},
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"PricingProfileDeleteError : {exception}");
        }
    }

    public async Task<PricingProfileDto> GetByIdAsync(GetPricingProfileByIdQuery request)
    {
        var pricingProfile = await _repository.GetAll()
                        .Include(b => b.Currency)
                        .Include(s => s.PricingProfileItems
                        .OrderBy(b => b.InstallmentNumberEnd))
                        .ThenInclude(a => a.PricingProfileInstallments.OrderBy(l => l.InstallmentSequence))
                        .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingProfile), request.Id);
        }

        return _mapper.Map<PricingProfileDto>(pricingProfile);
    }

    public async Task<PaginatedList<PricingProfileDto>> GetFilterListAsync(GetFilterPricingProfileQuery request)
    {
        var pricingProfileList = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            pricingProfileList = pricingProfileList.Where(b => b.Name.ToLower().Contains(request.Q.ToLower()));
        }

        if (request.ProfileStatus is not null)
        {
            pricingProfileList = pricingProfileList.Where(b => b.ProfileStatus
                               == request.ProfileStatus);
        }

        if (request.ProfileType is not null)
        {
            pricingProfileList = pricingProfileList.Where(b => b.ProfileType
                               == request.ProfileType);
        }

        if (request.CreateDateStart is not null)
        {
            pricingProfileList = pricingProfileList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            pricingProfileList = pricingProfileList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.PricingProfileNumber is not null)
        {
            pricingProfileList = pricingProfileList.Where(b => b.PricingProfileNumber
                               == request.PricingProfileNumber);
        }

        if (request.IsPaymentToMainMerchant is not null)
        {
            pricingProfileList = pricingProfileList.Where(b => b.IsPaymentToMainMerchant
                                                               == request.IsPaymentToMainMerchant);
        }

        return await pricingProfileList.Include(b => b.PricingProfileItems).ThenInclude(a => a.PricingProfileInstallments)
            .PaginatedListWithMappingAsync<PricingProfile,PricingProfileDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

    }

    public async Task SaveAsync(SavePricingProfileCommand request)
    {
        var activePricingProfileName = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Name.Equals(request.Name)
            && b.RecordStatus == RecordStatus.Active);

        if (activePricingProfileName is not null)
        {
            throw new DuplicateRecordException(request.Name);
        }

        var activePricingProfile = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.ActivationDate == request.ActivationDate
            && b.PerTransactionFee == request.PerTransactionFee
            && b.Name.Equals(request.Name)
            && b.RecordStatus == RecordStatus.Active);

        if (activePricingProfile is not null)
        {
            throw new DuplicateRecordException(nameof(PricingProfile));
        }

        foreach (var profileItem in request.PricingProfileItems)
        {
            var totalCommissionRate = profileItem.CommissionRate + profileItem.ParentMerchantCommissionRate;
            if (profileItem.IsActive && (request.IsPaymentToMainMerchant && totalCommissionRate != 100m ) ||
                (!request.IsPaymentToMainMerchant && totalCommissionRate >= 100m ))
            {
                throw new InvalidCommissionRateException();
            }
        }

        ValidateInstallment(request.PricingProfileItems);

        var generateProfileNumber = Generate();

        var items = _mapper.Map<List<PricingProfileItem>>(request.PricingProfileItems);

        var profile = _mapper.Map<PricingProfile>(request);
        profile.ProfileStatus = ProfileStatus.Waiting;
        profile.PricingProfileNumber = generateProfileNumber;
        profile.CurrencyCode = "TRY";
        profile.PricingProfileItems = items;

        await _repository.AddAsync(profile);
    }

    public async Task UpdateAsync(UpdatePricingProfileCommand request)
    {
        var pricingProfile = await _repository.GetAll()
                    .Include(s => s.PricingProfileItems)
                    .ThenInclude(a => a.PricingProfileInstallments)
                    .FirstOrDefaultAsync(b => b.Id == request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(pricingProfile));
        }

        if (pricingProfile.Name != request.Name)
        {
            var activePricingProfileName = await _repository.GetAll()
                .FirstOrDefaultAsync(b => b.Name.Equals(request.Name)
                                          && b.RecordStatus == RecordStatus.Active);

            if (activePricingProfileName is not null)
            {
                throw new DuplicateRecordException(request.Name);
            }
        }

        if (pricingProfile.ProfileType == ProfileType.Standard &&
            request.PricingProfileItems.Any(s => s.ParentMerchantCommissionRate > 0))
        {
            throw new ParentMerchantCommissionMustBeZeroException();
        }

        ValidateInstallment(request.PricingProfileItems);

        await ValidateActivationDateAsync(pricingProfile.PricingProfileNumber, request);

        if (request.ActivationDate != pricingProfile.ActivationDate)
        {
            try
            {
                if (pricingProfile.ProfileStatus == ProfileStatus.Waiting && request.ActivationDate > DateTime.Now)
                {
                    pricingProfile.ActivationDate = request.ActivationDate;
                    await UpdatePricingProfile(pricingProfile, request);
                }
                else
                {
                    var items = _mapper.Map<List<PricingProfileItem>>(request.PricingProfileItems);
                    items.ForEach(b =>
                    {
                        b.Id = Guid.Empty;
                        b.PricingProfileId = Guid.Empty;
                        b.PricingProfileInstallments.ForEach(s =>
                        {
                            s.Id = Guid.Empty;
                            s.PricingProfileItemId = Guid.Empty;
                        });
                    });

                    foreach (var requestItem in items)
                    {
                        var totalCommissionRate = requestItem.CommissionRate + requestItem.ParentMerchantCommissionRate;
                        if (requestItem.IsActive && (pricingProfile.IsPaymentToMainMerchant && totalCommissionRate != 100m ) ||
                            (!pricingProfile.IsPaymentToMainMerchant && totalCommissionRate >= 100m ))
                        {
                            throw new InvalidCommissionRateException();
                        }
                    }
                    
                    var profile = new PricingProfile
                    {
                        Name = request.Name,
                        PricingProfileNumber = pricingProfile.PricingProfileNumber,
                        ProfileStatus = ProfileStatus.Waiting,
                        ProfileType = pricingProfile.ProfileType,
                        ActivationDate = request.ActivationDate,
                        PerTransactionFee = request.PerTransactionFee,
                        IsPaymentToMainMerchant = pricingProfile.IsPaymentToMainMerchant,
                        CurrencyCode = "TRY",
                        PricingProfileItems = items,
                    };

                    await _repository.AddAsync(profile);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileUpdateError : {exception}");
                throw;
            }
        }
        else
        {
            try
            {
                await UpdatePricingProfile(pricingProfile, request);
            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileUpdateError : {exception}");
                throw;
            }
        }
        await _auditLogService.AuditLogAsync(
          new AuditLog
          {
              IsSuccess = true,
              LogDate = DateTime.Now,
              Operation = "UpdatePricingProfile",
              SourceApplication = "PF",
              Resource = "PricingProfile",
              UserId = parseUserId,
              Details = new Dictionary<string, string>
             {
             {"Id", request.Id.ToString()},
             {"Name", request.Name},
             {"ActivationDate", request.ActivationDate.ToString()},
             {"PerTransactionFee", request.PerTransactionFee.ToString()},
             }
          });
    }

    private async Task UpdatePricingProfile(PricingProfile pricingProfile, UpdatePricingProfileCommand request)
    {
        pricingProfile.Name = request.Name;
        pricingProfile.PerTransactionFee = request.PerTransactionFee;

        await _repository.UpdateAsync(pricingProfile);

        foreach (var profileItem in pricingProfile.PricingProfileItems)
        {
            var requestItem = request.PricingProfileItems.FirstOrDefault(x => x.Id == profileItem.Id);
            if (requestItem == null)
                continue;
            
            profileItem.ProfileCardType = requestItem.ProfileCardType;
            profileItem.InstallmentNumber = requestItem.InstallmentNumber;
            profileItem.InstallmentNumberEnd = requestItem.InstallmentNumberEnd;
            profileItem.BlockedDayNumber = requestItem.BlockedDayNumber;
            profileItem.CommissionRate = requestItem.CommissionRate;
            profileItem.ParentMerchantCommissionRate = requestItem.ParentMerchantCommissionRate;
            profileItem.IsActive = requestItem.IsActive;
            
            var totalCommissionRate = requestItem.CommissionRate + requestItem.ParentMerchantCommissionRate;
            if (requestItem.IsActive && (pricingProfile.IsPaymentToMainMerchant && totalCommissionRate != 100m ) ||
                (!pricingProfile.IsPaymentToMainMerchant && totalCommissionRate >= 100m ))
            {
                throw new InvalidCommissionRateException();
            }
            
            await _itemRepository.UpdateAsync(profileItem);
            
            foreach (var requestSettlement in requestItem.PricingProfileInstallments)
            {
                var existingSettlement = profileItem.PricingProfileInstallments
                    .FirstOrDefault(s => s.Id == requestSettlement.Id);

                if (existingSettlement is null) 
                    continue;
                
                existingSettlement.InstallmentSequence = requestSettlement.InstallmentSequence;
                existingSettlement.BlockedDayNumber = requestSettlement.BlockedDayNumber;
                await _itemInstallmentRepository.UpdateAsync(existingSettlement);
            }
        }
    }

    public string Generate()
    {
        var any = false;
        var pricingProfileNumber = string.Empty;

        var random = new Random();
        do
        {
            pricingProfileNumber  = _randomGenerator.GenerateSecureRandomNumber(6).ToString(CultureInfo.InvariantCulture);
            any = _repository.GetAll().Any(s => s.PricingProfileNumber == pricingProfileNumber);
        }
        while (any);

        return pricingProfileNumber;
    }

    private async Task ValidateActivationDateAsync(string profileNumber, UpdatePricingProfileCommand request)
    {
        var pricingProfiles = await _repository.GetAll()
            .Where(b => b.PricingProfileNumber == profileNumber && b.Id != request.Id).ToListAsync();

        if (pricingProfiles.Any())
        {
            foreach (var item in pricingProfiles)
            {
                if (item.ActivationDate == request.ActivationDate)
                {
                    throw new DuplicateRecordException(nameof(PricingProfile), nameof(request.ActivationDate));
                }
            }
        }
    }

    public void ValidateInstallment(List<PricingProfileItemDto> pricingProfileItems)
    {
        if (pricingProfileItems.All(b => !b.IsActive))
        {
            throw new InvalidInstallmentException();
        }
        
        var activeInstallments = pricingProfileItems.Where(b => b.IsActive && b.InstallmentNumberEnd > 0).ToList();
        if (
            (
                from installment in activeInstallments 
                let sequences = installment.PricingProfileInstallments.Select(s => s.InstallmentSequence).OrderBy(x => x).ToList() 
                where !sequences.SequenceEqual(Enumerable.Range(1, installment.InstallmentNumberEnd)) 
                select installment
            )
            .Any())
        {
            throw new InvalidInstallmentException();
        }
    }
}
