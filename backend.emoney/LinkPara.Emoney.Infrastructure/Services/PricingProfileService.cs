using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.DeletePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.SavePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfileItem;
using LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileList;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.SharedModels.Persistence;
using LinkPara.Emoney.Infrastructure.Persistence;
using System.Transactions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Emoney.Application.Features.PricingProfiles;
using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.Emoney.Application.Commons.Exceptions;
using MassTransit;
using LinkPara.Emoney.Application.Commons.Helpers;

namespace LinkPara.Emoney.Infrastructure.Services;

public class PricingProfileService : IPricingProfileService
{
    private readonly ILogger<PricingProfileService> _logger;
    private readonly EmoneyDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<PricingProfile> _repository;

    public PricingProfileService(
        ILogger<PricingProfileService> logger,
        EmoneyDbContext context,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IParameterService parameterService,
        IMapper mapper,
        IGenericRepository<PricingProfile> repository)
    {
        _logger = logger;
        _context = context;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _parameterService = parameterService;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<PricingProfile> GetByIdAsync(Guid id)
    {
        var profile = await _context.PricingProfile
                .Include(s => s.Bank)
                .Include(s => s.Currency)
                .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s => s.Id == id);

        return profile;
    }

    public async Task<PricingProfileItemDto> GetCardTopupCommissionAsync()
    {
        var profile = await _context.PricingProfile
                .Include(s => s.PricingProfileItems)
                .Where(b => b.TransferType == TransferType.CreditCardTopup
                        && b.PricingProfileItems.Count > 0
                        && b.Status == PricingProfileStatus.InUse)
                .OrderByDescending(b => b.ActivationDateStart)
            .FirstOrDefaultAsync();

        if (profile == null || profile.PricingProfileItems == null)
        {
            throw new NotFoundException(nameof(PricingProfile));
        }

        var item = profile.PricingProfileItems.First();

        var response = new PricingProfileItemDto() { CommissionRate = item.CommissionRate, Fee = item.Fee };
        return response;
    }

    public async Task<PaginatedList<PricingProfileDto>> GetListAsync(GetPricingProfileListQuery request)
    {
        var profiles = _context.PricingProfile
                .Include(s => s.Bank)
                .Include(s => s.Currency)
                .Include(s => s.PricingProfileItems)
                .AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            profiles = profiles.Where(b => b.Name.ToLower().Contains(request.Q.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.CurrencyCode))
        {
            profiles = profiles.Where(b => b.Currency.Code.Contains(request.CurrencyCode));
        }

        if (request.BankCode is not null)
        {
            profiles = profiles.Where(b => b.Bank.Code == request.BankCode);
        }

        if (request.TransferType is not null)
        {
            profiles = profiles.Where(b => b.TransferType
                               == request.TransferType);

            if (request.TransferType == TransferType.CreditCardTopup)
            {
                profiles = profiles.Where(b => b.PricingProfileItems.Count > 0);
            }
        }
        else
        {
            profiles = profiles.Where(b => b.TransferType
                               != TransferType.CreditCardTopup);
        }

        if (request.Status is not null)
        {
            profiles = profiles.Where(b => b.Status
                               == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            profiles = profiles.Where(x => x.ActivationDateStart >= request.StartDate);
        }
        if (request.EndDate.HasValue)
        {
            profiles = profiles.Where(x => x.ActivationDateStart <= request.EndDate);
        }
        if (request.CardType is not null)
        {
            profiles = profiles.Where(b => b.CardType
                               == request.CardType);
        }

        return await profiles.PaginatedListWithMappingAsync<PricingProfile, PricingProfileDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SavePricingProfileCommand request)
    {
        if (await IsExistAsync(request))
            throw new DuplicateRecordException();

        if ((request.TransferType == TransferType.Internal || request.TransferType == TransferType.CreditCardTopup || request.TransferType == TransferType.PaymentWithWallet)
            && request.BankCode is not null)
        {
            throw new InvalidParameterException($"{nameof(TransferType)} or {nameof(request.BankCode)}");
        }

        var items = request.ProfileItems.Select(x => new PricingProfileItem
        {
            CommissionRate = x.CommissionRate,
            Fee = x.Fee,
            MaxAmount = x.MaxAmount,
            MinAmount = x.MinAmount,
            WalletType = x.WalletType,
            CreateDate = DateTime.Now,
            CreatedBy = GetUserId().ToString()
        }).ToList();

        var profileName = await GenerateProfileNameAsync(request.BankCode, request.CurrencyCode, request.TransferType);
        
        CardType cardType = CardType.Unknown;
        if (request.TransferType == TransferType.CreditCardTopup)
        {
            cardType = request.CardType == CardType.Unknown ? CardType.Credit : request.CardType;
        }

        var profile = new PricingProfile
        {
            Status = PricingProfileStatus.Waiting,
            ActivationDateEnd = null,
            ActivationDateStart = request.ActivationDateStart,
            BankCode = request.BankCode,
            CurrencyCode = request.CurrencyCode.ToUpper(),
            Name = profileName,
            TransferType = request.TransferType,
            PricingProfileItems = items,
            CreateDate = DateTime.Now,
            CreatedBy = GetUserId().ToString(),
            CardType = cardType
        };

        await _context.PricingProfile.AddAsync(profile);

        await _context.SaveChangesAsync();

        await PricingProfileAuditLogAsync(true, "SavePricingProfile", new Dictionary<string, string>
        {
            {"PricingProfileId",profile.Id.ToString() },
            {"ActivationDateStart",request.ActivationDateStart.ToString() },
            {"BankCode",request.BankCode.ToString() },
            {"CurrencyCode",request.CurrencyCode },
            {"CardType",request.CardType.ToString() },
            {"PricingProfileItemIds-ComisionRates-MinAmounts-MaxAmounts",string.Join(',',items.Select(x => $"{x.Id}-{x.CommissionRate}-{x.MinAmount}-{x.MaxAmount}" ).ToList())}
        });
    }
    private async Task<bool> IsExistAsync(SavePricingProfileCommand request)
    {
        if (request.TransferType == TransferType.CreditCardTopup)
        {
            return false;
        }

        return await _repository
            .GetAll()
            .AnyAsync(x =>
                x.BankCode == request.BankCode &&
                x.CurrencyCode == request.CurrencyCode &&
                x.TransferType == request.TransferType &&
                x.CardType == request.CardType
        );
    }
    public async Task UpdateAsync(UpdatePricingProfileCommand request)
    {
        if ((request.TransferType == TransferType.Internal || request.TransferType == TransferType.CreditCardTopup || request.TransferType == TransferType.PaymentWithWallet)
            && request.BankCode is not null)
        {
            throw new InvalidParameterException($"{nameof(TransferType)} or {nameof(request.BankCode)}");
        }

        var profile = await _context.PricingProfile
                .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s => s.Id == request.Id);

        if (profile.ActivationDateEnd is not null || profile.RecordStatus == RecordStatus.Passive)
        {
            throw new ProfileIsPassiveException();
        }

        var isActive =
            profile.Status == PricingProfileStatus.InUse &&
            profile.RecordStatus == RecordStatus.Active &&
            profile.ActivationDateEnd is null &&
            profile.ActivationDateStart <= DateTime.Now;

        if (isActive)
        {
            CheckProfileItemAmount(profile.PricingProfileItems, request.ProfileItems.FirstOrDefault());

            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    var pricingProfileList = new List<PricingProfileItem>();
                    var items = request.ProfileItems.Select(x => new PricingProfileItem
                    {
                        CommissionRate = x.CommissionRate,
                        Fee = x.Fee,
                        MaxAmount = x.MaxAmount,
                        MinAmount = x.MinAmount,
                        WalletType = x.WalletType,
                        CreatedBy = GetUserId().ToString(),
                        CreateDate = DateTime.Now
                    }).ToList();

                    pricingProfileList.AddRange(items);
                    profile.PricingProfileItems.ForEach(b => b.Id = Guid.Empty);
                    pricingProfileList.AddRange(profile.PricingProfileItems);

                    var profileName = await GenerateProfileNameAsync(request.BankCode, request.CurrencyCode, request.TransferType);

                    var newProfile = new PricingProfile
                    {
                        Status = PricingProfileStatus.Waiting,
                        ActivationDateEnd = null,
                        ActivationDateStart = request.ActivationDateStart,
                        BankCode = request.BankCode,
                        CurrencyCode = request.CurrencyCode.ToUpper(),
                        Name = profileName,
                        TransferType = request.TransferType,
                        PricingProfileItems = pricingProfileList,
                        CreatedBy = GetUserId().ToString(),
                        CreateDate = DateTime.Now
                    };

                    await _context.PricingProfile.AddAsync(newProfile);

                    await _context.SaveChangesAsync();

                    scope.Complete();

                    await PricingProfileAuditLogAsync(true, "UpdatePricingProfile", new Dictionary<string, string>
                    {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"ActivationDateStart",request.ActivationDateStart.ToString() },
                    {"BankCode",request.BankCode.ToString() },
                    {"CurrencyCode",request.CurrencyCode },
                    {"PricingProfileItemIds-ComisionRates-MinAmounts-MaxAmounts",string.Join(',',items.Select(x => $"{x.Id}-{x.CommissionRate}-{x.MinAmount}-{x.MaxAmount}" ).ToList())}
                    });

                    await _auditLogService.AuditLogAsync(
                        new AuditLog
                        {
                            IsSuccess = true,
                            LogDate = DateTime.Now,
                            Operation = "AddPricingProfile",
                            SourceApplication = "Emoney",
                            Resource = "PricingProfile",
                            Details = new Dictionary<string, string>
                            {
                               {"Id", newProfile.Id.ToString() },
                               {"Name", newProfile.Name },
                               {"BankCode", newProfile.BankCode.ToString() }
                            }
                        });
                });

            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileUpdateError : {exception}");

                await PricingProfileAuditLogAsync(false, "UpdatePricingProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"Error",exception.Message },
                });
            }
        }
        else
        {
            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    _context.PricingProfile.Attach(profile);

                    profile.Status = PricingProfileStatus.Waiting;
                    profile.ActivationDateEnd = null;
                    profile.ActivationDateStart = request.ActivationDateStart;
                    profile.BankCode = request.BankCode;
                    profile.CurrencyCode = request.CurrencyCode.ToUpper();
                    profile.TransferType = request.TransferType;
                    profile.LastModifiedBy = GetUserId().ToString();
                    profile.UpdateDate = DateTime.Now;

                    await _context.PricingProfileItem.AddRangeAsync(request.ProfileItems.Select(x =>
                        new PricingProfileItem
                        {
                            CreateDate = DateTime.Now,
                            CommissionRate = x.CommissionRate,
                            Fee = x.Fee,
                            MaxAmount = x.MaxAmount,
                            MinAmount = x.MinAmount,
                            WalletType = x.WalletType,
                            PricingProfileId = profile.Id,
                            LastModifiedBy = GetUserId().ToString(),
                            RecordStatus = RecordStatus.Active,
                            UpdateDate = DateTime.Now,
                            CreatedBy = GetUserId().ToString()
                        }));

                    await _context.SaveChangesAsync();

                    scope.Complete();

                    await PricingProfileAuditLogAsync(true, "UpdatePricingProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"ActivationDateStart",request.ActivationDateStart.ToString() },
                    {"BankCode",request.BankCode.ToString() },
                    {"CurrencyCode",request.CurrencyCode },
                    {"PricingProfileItemIds-ComisionRates-MinAmounts-MaxAmounts",string.Join(',',profile.PricingProfileItems.Select(x => $"{x.Id}-{x.CommissionRate}-{x.MinAmount}-{x.MaxAmount}" ).ToList())}
                });

                    await _auditLogService.AuditLogAsync(
                        new AuditLog
                        {
                            IsSuccess = true,
                            LogDate = DateTime.Now,
                            Operation = "UpdatePricingProfile",
                            SourceApplication = "Emoney",
                            Resource = "PricingProfile",
                            Details = new Dictionary<string, string>
                            {
                               {"Id", profile.Id.ToString() },
                               {"Name", profile.Name },
                               {"BankCode", profile.BankCode.ToString() },
                               {"CurrencyCode", profile.CurrencyCode },
                               {"TransferType", profile.TransferType.ToString() },
                               {"Status", profile.Status.ToString() }
                            }
                        });
                });
            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileUpdateError : {exception}");

                await PricingProfileAuditLogAsync(false, "UpdatePricingProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"Error",exception.Message },
                });
            }
        }
    }

    public async Task UpdateProfileItemAsync(UpdatePricingProfileItemCommand request)
    {
        var item = await _context.PricingProfileItem.FirstOrDefaultAsync(s => s.Id == request.Id);

        var profile = await _context.PricingProfile.Include(s => s.PricingProfileItems).FirstOrDefaultAsync(s => s.Id == item.PricingProfileId);

        if (profile.ActivationDateEnd is not null || profile.RecordStatus == RecordStatus.Passive)
        {
            await PricingProfileAuditLogAsync(false, "UpdatePricingItemProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"Message","Profile is passive" },
                });
            throw new ProfileIsPassiveException();
        }

        var isActive =
            profile.Status == PricingProfileStatus.InUse &&
            profile.RecordStatus == RecordStatus.Active &&
            profile.ActivationDateEnd is null &&
            profile.ActivationDateStart <= DateTime.Now;

        if (isActive)
        {
            CheckProfileItemAmount(profile.PricingProfileItems.Where(b => b.Id != request.Id).ToList(),
                new PricingProfileItemUpdateModel { MaxAmount = request.MaxAmount, MinAmount = request.MinAmount });

            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    var newItems = new List<PricingProfileItem>
                    {
                        new PricingProfileItem
                        {
                            CommissionRate = request.CommissionRate,
                            Fee = request.Fee,
                            MaxAmount = request.MaxAmount,
                            MinAmount = request.MinAmount,
                            WalletType = request.WalletType,
                            CreatedBy = GetUserId().ToString(),
                            CreateDate = DateTime.Now
                        }
                    };

                    foreach (var profileItem in profile.PricingProfileItems)
                    {
                        if (profileItem.Id != request.Id)
                        {
                            newItems.Add(new PricingProfileItem
                            {
                                CommissionRate = profileItem.CommissionRate,
                                Fee = profileItem.Fee,
                                MaxAmount = profileItem.MaxAmount,
                                MinAmount = profileItem.MinAmount,
                                WalletType = profileItem.WalletType,
                                CreatedBy = GetUserId().ToString(),
                                CreateDate = DateTime.Now
                            });
                        }
                    }

                    DisableProfile(profile, PricingProfileStatus.Finished);

                    var profileName = await GenerateProfileNameAsync(profile.BankCode, profile.CurrencyCode, profile.TransferType);

                    CardType cardType = CardType.Unknown;
                    if (profile.TransferType == TransferType.CreditCardTopup)
                    {
                        cardType = profile.CardType == CardType.Unknown ? CardType.Credit : profile.CardType;
                    }

                    var newProfile = new PricingProfile
                    {
                        Status = PricingProfileStatus.InUse,
                        ActivationDateEnd = null,
                        ActivationDateStart = DateTime.Now,
                        BankCode = profile.BankCode,
                        CurrencyCode = profile.CurrencyCode.ToUpper(),
                        Name = profileName,
                        TransferType = profile.TransferType,
                        PricingProfileItems = newItems,
                        CreatedBy = GetUserId().ToString(),
                        CreateDate = DateTime.Now,
                        CardType = cardType
                    };

                    await _context.PricingProfile.AddAsync(newProfile);

                    await _context.SaveChangesAsync();

                    await PricingProfileAuditLogAsync(true, "UpdatePricingItemProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"ActivationDateStart",profile.ActivationDateStart.ToString() },
                    {"BankCode",profile.BankCode.ToString() },
                    {"CurrencyCode",profile.CurrencyCode },
                    {"PricingProfileItemIds-ComisionRates-MinAmounts-MaxAmounts",string.Join(',',profile.PricingProfileItems.Select(x => $"{x.Id}-{x.CommissionRate}-{x.MinAmount}-{x.MaxAmount}" ).ToList())}
                });

                    scope.Complete();
                });


            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileUpdateError : {exception}");

                await PricingProfileAuditLogAsync(false, "UpdatePricingItemProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"Error",exception.Message },
                });
            }
        }
        else
        {
            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    _context.PricingProfileItem.Attach(item);

                    item.CommissionRate = request.CommissionRate;
                    item.Fee = request.Fee;
                    item.MaxAmount = request.MaxAmount;
                    item.MinAmount = request.MinAmount;
                    item.WalletType = request.WalletType;
                    item.UpdateDate = DateTime.Now;
                    item.LastModifiedBy = GetUserId().ToString();

                    await _context.SaveChangesAsync();

                    scope.Complete();

                    await PricingProfileAuditLogAsync(true, "UpdatePricingItemProfile", new Dictionary<string, string>
                     {
                         {"PricingProfileId",profile.Id.ToString() },
                         {"ActivationDateStart",profile.ActivationDateStart.ToString() },
                         {"BankCode",profile.BankCode.ToString() },
                         {"CurrencyCode",profile.CurrencyCode },
                         {"PricingProfileItemIds-ComisionRates-MinAmounts-MaxAmounts",string.Join(',',profile.PricingProfileItems.Select(x => $"{x.Id}-     {x.CommissionRate}-{x.MinAmount}-{x.MaxAmount}" ).ToList())}
                     });
                });

            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileUpdateError : {exception}");

                await PricingProfileAuditLogAsync(false, "UpdatePricingItemProfile", new Dictionary<string, string>
                {
                    {"PricingProfileId",profile.Id.ToString() },
                    {"Error",exception.Message },
                });
            }
        }
    }

    public async Task DeleteAsync(DeletePricingProfileItemCommand request)
    {
        var profileItem = await _context.PricingProfileItem
            .FirstOrDefaultAsync(s => s.Id == request.Id);

        if (profileItem is null)
        {
            await PricingProfileAuditLogAsync(false, "DeletePricingProfileItem", new Dictionary<string, string>
            {
                {"PricingProfileItemId",request.Id.ToString() },
                {"Message","NotFound"},
            });

            throw new NotFoundException(nameof(PricingProfile), request.Id);
        }

        var profile = await _context.PricingProfile
            .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s => s.Id == profileItem.PricingProfileId);


        if (profile is null)
        {
            await PricingProfileAuditLogAsync(false, "DeletePricingProfileItem", new Dictionary<string, string>
            {
                {"PricingProfileId",request.Id.ToString() },
                {"Message","NotFound"},
            });
            throw new NotFoundException(nameof(PricingProfile), request.Id);
        }

        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (profile.RecordStatus == RecordStatus.Active)
                {
                    var newProfile = new PricingProfile
                    {
                        Status = profile.Status,
                        ActivationDateEnd = null,
                        ActivationDateStart = profile.Status == PricingProfileStatus.InUse ? DateTime.Now : profile.ActivationDateStart,
                        BankCode = profile.BankCode,
                        CurrencyCode = profile.CurrencyCode.ToUpper(),
                        Name = profile.Name,
                        TransferType = profile.TransferType,
                        PricingProfileItems = profile.PricingProfileItems
                            .Where(s => s.Id != request.Id)
                            .Select(x =>
                                new PricingProfileItem
                                {
                                    CreateDate = DateTime.Now,
                                    CommissionRate = x.CommissionRate,
                                    Fee = x.Fee,
                                    MaxAmount = x.MaxAmount,
                                    MinAmount = x.MinAmount,
                                    WalletType = x.WalletType,
                                    PricingProfileId = profile.Id,
                                    LastModifiedBy = GetUserId().ToString(),
                                    RecordStatus = RecordStatus.Active,
                                    UpdateDate = DateTime.Now,
                                    CreatedBy = GetUserId().ToString()
                                })
                            .ToList(),
                        CreatedBy = GetUserId().ToString(),
                        CreateDate = DateTime.Now
                    };

                    await _context.PricingProfile.AddAsync(newProfile);

                    DisableProfile(profile, PricingProfileStatus.Finished);
                }
                else
                {
                    throw new ProfileIsPassiveException();
                }

                await _context.SaveChangesAsync();

                await PricingProfileAuditLogAsync(true, "DeletePricingProfile", new Dictionary<string, string>
            {
                {"PricingProfileId",profile.Id.ToString() }
            });

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeletePricingProfile",
                    SourceApplication = "Emoney",
                    Resource = "PricingProfile",
                    Details = new Dictionary<string, string>
                    {
                       {"Id", profile.Id.ToString() },
                       {"Name", profile.Name }
                    }
                });

                scope.Complete();
            });
        }
        catch (Exception exception)
        {

            _logger.LogError($"PricingProfileDeleteError : {exception}");

            await PricingProfileAuditLogAsync(false, "DeletePricingProfile", new Dictionary<string, string>
            {
                {"PricingProfileId",profile.Id.ToString() },
                {"Error",exception.Message },
            });
        }

    }

    public async Task<CalculatePricingResponse> CalculatePricingAsync(CalculatePricingRequest request)
    {
        var profile = await _context.PricingProfile
                .Include(s => s.Bank)
                .Include(s => s.Currency)
                .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s =>
                s.Status == PricingProfileStatus.InUse &&
                s.TransferType == request.TransferType &&
                s.BankCode == request.BankCode &&
                s.CurrencyCode == request.CurrencyCode.ToUpper());

        if (profile is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var item = profile.PricingProfileItems.FirstOrDefault(s =>
            s.MinAmount <= request.Amount &&
            s.MaxAmount >= request.Amount &&
            s.WalletType == request.SenderWalletType);

        if (item is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var amount = request.Amount.ToDecimal2();
        var fee = item.Fee.ToDecimal2();
        var commissionRate = item.CommissionRate.ToDecimal2();
        var commissionAmount = (amount * (commissionRate / 100m)).ToDecimal2();
        var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
        var bsmvRate = Convert.ToDecimal(bsmvRateParameter?.ParameterValue);

        var response = new CalculatePricingResponse
        {
            Amount = amount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            Fee = fee,
            BsmvRate = bsmvRate,
            BsmvTotal = ((fee + commissionAmount) * (bsmvRate / 100m)).ToDecimal2(),
        };

        if (response.BsmvTotal == 0 && response.CommissionAmount > 0)
        {
            response.BsmvTotal = 0.01m;
        }

        return response;
    }

    public async Task CheckProfileStatus()
    {
        var waitingProfiles = await _context.PricingProfile
            .Where(s => s.Status == PricingProfileStatus.Waiting && s.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        foreach (var profile in waitingProfiles)
        {
            await ActivationDate(profile);
        }
    }
    private void CheckProfileItemAmount(List<PricingProfileItem> profileItems, PricingProfileItemUpdateModel requestItem)
    {
        foreach (var item in profileItems)
        {
            bool isOverlap =
                (requestItem.MinAmount >= item.MinAmount && requestItem.MinAmount <= item.MaxAmount) ||
                (requestItem.MaxAmount <= item.MaxAmount && requestItem.MaxAmount >= item.MinAmount) ||
                (requestItem.MinAmount >= item.MinAmount && requestItem.MaxAmount <= item.MaxAmount) ||
                (requestItem.MinAmount <= item.MinAmount && requestItem.MaxAmount >= item.MaxAmount);

            if (isOverlap && requestItem.WalletType == item.WalletType)
            {
                throw new AmountIntervalIsAlreadyInUseException();
            }
        }
    }

    private async Task ActivationDate(PricingProfile profile)
    {
        if (profile.ActivationDateStart <= DateTime.Now)
        {
            var currentProfile = await _context.PricingProfile
                .Include(b => b.PricingProfileItems)
                .FirstOrDefaultAsync(s =>
                    s.Status == PricingProfileStatus.InUse &&
                    s.BankCode == profile.BankCode &&
                    s.CurrencyCode == profile.CurrencyCode &&
                    s.TransferType == profile.TransferType &&
                    s.CardType == profile.CardType);

            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    if (currentProfile is not null)
                    {
                        DisableProfile(currentProfile, PricingProfileStatus.Finished);
                    }

                    _context.PricingProfile.Attach(profile);
                    profile.UpdateDate = DateTime.Now;
                    profile.Status = PricingProfileStatus.InUse;
                    profile.ActivationDateStart = DateTime.Now;

                    await _context.SaveChangesAsync();

                    scope.Complete();
                });


                await PricingProfileAutomatedChangeAuditLogAsync(true, profile.CreatedBy, new Dictionary<string, string>
                    {
                        {"DisabledProfileId", currentProfile is not null ? currentProfile.Id.ToString() : String.Empty},
                        {"ActivatedProfileId",profile.Id.ToString() }
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError($"PricingProfileDeleteError : {exception}");
                await PricingProfileAutomatedChangeAuditLogAsync(false, profile.CreatedBy, new Dictionary<string, string>
                    {
                        {"DisabledProfileId", currentProfile is not null ? currentProfile.Id.ToString() : String.Empty},
                        {"ActivatedProfileId",profile.Id.ToString() },
                        {"ErrorMessage",exception.Message }
                    });
            }
        }
    }

    private void DisableProfile(PricingProfile profile, PricingProfileStatus status)
    {
        _context.PricingProfile.Attach(profile);

        profile.RecordStatus = RecordStatus.Passive;
        profile.ActivationDateEnd = DateTime.Now;
        profile.Status = status;
        profile.LastModifiedBy = GetUserId().ToString();

        foreach (var item in profile.PricingProfileItems)
        {
            item.RecordStatus = RecordStatus.Passive;
        }
    }

    private async Task<string> GenerateProfileNameAsync(int? bankCode, string currencyCode, TransferType transferType)
    {
        currencyCode = currencyCode.ToUpper();

        var currency = await _context.Currency.FirstOrDefaultAsync(s => s.Code == currencyCode);

        if (transferType == TransferType.Internal || transferType == TransferType.PaymentWithWallet)
        {
            return $"P2P_{currency.Code}_{transferType}";
        }
        else if (transferType == TransferType.CreditCardTopup)
        {
            return $"Topup_{currency.Code}_{transferType}";
        }
        else
        {
            var bank = await _context.Bank.FirstOrDefaultAsync(s => s.Code == bankCode);
            return $"{bank.Name}_{currency.Code}_{transferType}";
        }
    }

    private async Task PricingProfileAuditLogAsync(bool isSuccess, string operation, Dictionary<string, string> deatils)
    {
        var userId = GetUserId();
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = operation,
                Resource = "PricingProfile",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }

    private async Task PricingProfileAutomatedChangeAuditLogAsync(bool isSuccess, string userId, Dictionary<string, string> deatils)
    {
        if (Guid.TryParse(userId, out var id))
        {
            id = Guid.Empty;
        }
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "CheckProfileStatus",
                Resource = "PricingProfile",
                SourceApplication = "Emoney",
                UserId = id
            }
        );
    }

    private Guid GetUserId()
    {
        if (!Guid.TryParse(_contextProvider.CurrentContext.UserId, out Guid userId))
        {
            //UnknownUser
            userId = Guid.Empty;
        }
        return userId;
    }

}
