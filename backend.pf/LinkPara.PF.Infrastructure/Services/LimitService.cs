using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Limit;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LinkPara.PF.Infrastructure.Services
{
    public class LimitService : ILimitService
    {
        private readonly IGenericRepository<MerchantLimit> _merchantLimitRepository;
        private readonly IGenericRepository<MerchantDailyUsage> _merchantDailyUsageRepository;
        private readonly IGenericRepository<MerchantMonthlyUsage> _merchantMonthlyUsageRepository;
        private readonly IGenericRepository<SubMerchantLimit> _subMerchantLimitRepository;
        private readonly IGenericRepository<SubMerchantDailyUsage> _subMerchantDailyUsageRepository;
        private readonly IGenericRepository<SubMerchantMonthlyUsage> _subMerchantMonthlyUsageRepository;
        private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILogger<LimitService> _logger;
        private readonly IGenericRepository<Currency> _currencyRepository;
        private readonly IResponseCodeService _errorCodeService;

        public LimitService(IGenericRepository<MerchantLimit> merchantLimitRepository,
            IGenericRepository<MerchantDailyUsage> merchantDailyUsageRepository,
            IGenericRepository<MerchantMonthlyUsage> merchantMonthlyUsageRepository,
            IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
            IApplicationUserService applicationUserService,
            ILogger<LimitService> logger,
            IGenericRepository<Currency> currencyRepository,
            IResponseCodeService errorCodeService,
            IGenericRepository<SubMerchantLimit> subMerchantLimitRepository,
            IGenericRepository<SubMerchantDailyUsage> subMerchantDailyUsageRepository,
            IGenericRepository<SubMerchantMonthlyUsage> subMerchantMonthlyUsageRepository)
        {
            _merchantLimitRepository = merchantLimitRepository;
            _merchantDailyUsageRepository = merchantDailyUsageRepository;
            _merchantMonthlyUsageRepository = merchantMonthlyUsageRepository;
            _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
            _applicationUserService = applicationUserService;
            _logger = logger;
            _currencyRepository = currencyRepository;
            _errorCodeService = errorCodeService;
            _subMerchantLimitRepository = subMerchantLimitRepository;
            _subMerchantDailyUsageRepository = subMerchantDailyUsageRepository;
            _subMerchantMonthlyUsageRepository = subMerchantMonthlyUsageRepository;
        }

        public async Task<ValidationResponse> CheckLimitAsync(CheckLimitRequest request)
        {
            if (request.TransactionType is TransactionType.PostAuth)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidTransactionType, request.LanguageCode);
            }

            var transactionType = ConvertToTransactionLimitType(request.TransactionType);

            var activeMerchantLimits = await _merchantLimitRepository
               .GetAll()
               .Where(b => b.MerchantId == request.MerchantId
                        && b.RecordStatus == RecordStatus.Active
                        && b.TransactionLimitType == transactionType)
               .ToListAsync();

            var activeSubMerchantLimits = new List<SubMerchantLimit>();
            if (request.SubMerchantId is not null)
            {
                activeSubMerchantLimits = await _subMerchantLimitRepository
                    .GetAll()
                    .Where(b => b.SubMerchantId == request.SubMerchantId
                       && b.RecordStatus == RecordStatus.Active
                       && b.TransactionLimitType == transactionType)
                    .ToListAsync();
            }

            if ((!activeMerchantLimits.Any() && !activeSubMerchantLimits.Any()) || (!activeMerchantLimits.Any() && !request.SubMerchantId.HasValue))
            {
                return new ValidationResponse { IsValid = true };
            }

            var merchantLimitResponse = await CheckMerchantLimit(request, activeMerchantLimits, transactionType);

            if (merchantLimitResponse.IsValid)
            {
                return await CheckSubMerchantLimit(request, activeSubMerchantLimits, transactionType);
            }

            return merchantLimitResponse;
        }
        private async Task<ValidationResponse> CheckMerchantLimit(CheckLimitRequest request, List<MerchantLimit> activeMerchantLimits, TransactionLimitType transactionType)
        {
            var activeMerchantDailyUsage = await _merchantDailyUsageRepository
               .GetAll()
               .SingleOrDefaultAsync(b => b.MerchantId == request.MerchantId
                        && b.RecordStatus == RecordStatus.Active
                        && b.Currency == request.Currency
                        && b.Date == DateTime.Now.Date
                        && b.TransactionLimitType == transactionType);

            var activeMerchantMonthlyUsage = await _merchantMonthlyUsageRepository
                .GetAll()
                .SingleOrDefaultAsync(b => b.MerchantId == request.MerchantId
                         && b.RecordStatus == RecordStatus.Active
                         && b.Date.Month == DateTime.Now.Month
                         && b.Date.Year == DateTime.Now.Year
                         && b.Currency == request.Currency
                         && b.TransactionLimitType == transactionType);

            if (activeMerchantDailyUsage != null)
            {
                var checkDailyCount = await CheckDailyLimitCountAsync(request, activeMerchantLimits, activeMerchantDailyUsage);
                if (!checkDailyCount.IsValid)
                {
                    return checkDailyCount;
                }
                var checkDailyAmount = await CheckDailyLimitAmountAsync(request, activeMerchantLimits, activeMerchantDailyUsage);
                if (!checkDailyAmount.IsValid)
                {
                    return checkDailyAmount;
                }
            }

            if (activeMerchantMonthlyUsage != null)
            {
                var checkMonthlyCount = await CheckMonthlyLimitCountAsync(request, activeMerchantLimits, activeMerchantMonthlyUsage);
                if (!checkMonthlyCount.IsValid)
                {
                    return checkMonthlyCount;
                }
                var checkMonthlyAmount = await CheckMonthlyLimitAmountAsync(request, activeMerchantLimits, activeMerchantMonthlyUsage);
                if (!checkMonthlyAmount.IsValid)
                {
                    return checkMonthlyAmount;
                }
            }

            if (activeMerchantDailyUsage is null && activeMerchantMonthlyUsage is null)
            {
                foreach (var item in activeMerchantLimits)
                {
                    if (item.LimitType == LimitType.Amount)
                    {
                        if (request.Amount > item.MaxAmount)
                        {
                            var validationResponse =
                                await GetValidationResponseAsync(ApiErrorCode.MerchantLimitExceeded,
                                    request.LanguageCode);

                            _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                                        $"Message: {validationResponse.Message}");

                            await InsertValidationLogAsync(request, validationResponse);

                            return validationResponse;
                        }
                    }
                }
            }

            return new ValidationResponse { IsValid = true };
        }

        private async Task<ValidationResponse> CheckSubMerchantLimit(CheckLimitRequest request, List<SubMerchantLimit> activeSubMerchantLimits, TransactionLimitType transactionType)
        {
            var activeSubMerchantDailyUsage = await _subMerchantDailyUsageRepository
               .GetAll()
               .SingleOrDefaultAsync(b => b.SubMerchantId == request.SubMerchantId
                        && b.RecordStatus == RecordStatus.Active
                        && b.Currency == request.Currency
                        && b.Date == DateTime.Now.Date
                        && b.TransactionLimitType == transactionType);

            var activeSubMerchantMonthlyUsage = await _subMerchantMonthlyUsageRepository
                .GetAll()
                .SingleOrDefaultAsync(b => b.SubMerchantId == request.SubMerchantId
                         && b.RecordStatus == RecordStatus.Active
                         && b.Date.Month == DateTime.Now.Month
                         && b.Date.Year == DateTime.Now.Year
                         && b.Currency == request.Currency
                         && b.TransactionLimitType == transactionType);

            if (activeSubMerchantDailyUsage != null)
            {
                var checkDailyCount = await CheckSubMerchantDailyLimitCountAsync(request, activeSubMerchantLimits, activeSubMerchantDailyUsage);
                if (!checkDailyCount.IsValid)
                {
                    return checkDailyCount;
                }
                var checkDailyAmount = await CheckSubMerchantDailyLimitAmountAsync(request, activeSubMerchantLimits, activeSubMerchantDailyUsage);
                if (!checkDailyAmount.IsValid)
                {
                    return checkDailyAmount;
                }
            }

            if (activeSubMerchantMonthlyUsage != null)
            {
                var checkMonthlyCount = await CheckSubMerchantMonthlyLimitCountAsync(request, activeSubMerchantLimits, activeSubMerchantMonthlyUsage);
                if (!checkMonthlyCount.IsValid)
                {
                    return checkMonthlyCount;
                }
                var checkMonthlyAmount = await CheckSubMerchantMonthlyLimitAmountAsync(request, activeSubMerchantLimits, activeSubMerchantMonthlyUsage);
                if (!checkMonthlyAmount.IsValid)
                {
                    return checkMonthlyAmount;
                }
            }

            if (activeSubMerchantDailyUsage is null && activeSubMerchantMonthlyUsage is null)
            {
                foreach (var item in activeSubMerchantLimits)
                {
                    if (item.LimitType == LimitType.Amount)
                    {
                        if (request.Amount > item.MaxAmount)
                        {
                            var validationResponse =
                                await GetValidationResponseAsync(ApiErrorCode.MerchantLimitExceeded,
                                    request.LanguageCode);

                            _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                                        $"Message: {validationResponse.Message}");

                            await InsertValidationLogAsync(request, validationResponse);

                            return validationResponse;
                        }
                    }
                }
            }

            return new ValidationResponse { IsValid = true };
        }

        private async Task<ValidationResponse> CheckSubMerchantDailyLimitCountAsync(CheckLimitRequest request, List<SubMerchantLimit> activeSubMerchantLimits,
            SubMerchantDailyUsage activeSubMerchantDailyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var dailyLimitCount = activeSubMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Daily
                                  && x.LimitType == LimitType.Count
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (dailyLimitCount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeSubMerchantDailyUsage.Count + 1;

            if (dailyLimitCount.MaxPiece < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.DailyLimitCountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return await GetValidationResponseAsync(ApiErrorCode.DailyLimitCountExceeded, request.LanguageCode);
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckSubMerchantDailyLimitAmountAsync(CheckLimitRequest request, List<SubMerchantLimit> activeSubMerchantLimits,
            SubMerchantDailyUsage activeSubMerchantDailyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var dailyLimitAmount = activeSubMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Daily
                                  && x.LimitType == LimitType.Amount
                                  && x.Currency == request.Currency
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (dailyLimitAmount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeSubMerchantDailyUsage.Amount + request.Amount;

            if (dailyLimitAmount.MaxAmount < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.DailyLimitAmountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return validationResponse;
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckSubMerchantMonthlyLimitCountAsync(CheckLimitRequest request, List<SubMerchantLimit> activeSubMerchantLimits,
            SubMerchantMonthlyUsage activeSubMerchantMonthlyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);


            var monthlyLimitCount = activeSubMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Monthly
                                  && x.LimitType == LimitType.Count
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (monthlyLimitCount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeSubMerchantMonthlyUsage.Count + 1;

            if (monthlyLimitCount.MaxPiece < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.MonthlyLimitCountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return validationResponse;
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckSubMerchantMonthlyLimitAmountAsync(CheckLimitRequest request, List<SubMerchantLimit> activeSubMerchantLimits,
            SubMerchantMonthlyUsage activeSubMerchantMonthlyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var monthlyLimitAmount = activeSubMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Monthly
                                  && x.LimitType == LimitType.Amount
                                  && x.Currency == request.Currency
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (monthlyLimitAmount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeSubMerchantMonthlyUsage.Amount + request.Amount;

            if (monthlyLimitAmount.MaxAmount < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.MonthlyLimitAmountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return validationResponse;
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckDailyLimitCountAsync(CheckLimitRequest request, List<MerchantLimit> activeMerchantLimits,
            MerchantDailyUsage activeMerchantDailyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var dailyLimitCount = activeMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Daily
                                  && x.LimitType == LimitType.Count
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (dailyLimitCount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeMerchantDailyUsage.Count + 1;

            if (dailyLimitCount.MaxPiece < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.DailyLimitCountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return await GetValidationResponseAsync(ApiErrorCode.DailyLimitCountExceeded, request.LanguageCode);
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckDailyLimitAmountAsync(CheckLimitRequest request, List<MerchantLimit> activeMerchantLimits,
            MerchantDailyUsage activeMerchantDailyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var dailyLimitAmount = activeMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Daily
                                  && x.LimitType == LimitType.Amount
                                  && x.Currency == request.Currency
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (dailyLimitAmount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeMerchantDailyUsage.Amount + request.Amount;

            if (dailyLimitAmount.MaxAmount < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.DailyLimitAmountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return validationResponse;
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckMonthlyLimitCountAsync(CheckLimitRequest request, List<MerchantLimit> activeMerchantLimits,
            MerchantMonthlyUsage activeMerchantMonthlyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);


            var monthlyLimitCount = activeMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Monthly
                                  && x.LimitType == LimitType.Count
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (monthlyLimitCount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeMerchantMonthlyUsage.Count + 1;

            if (monthlyLimitCount.MaxPiece < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.MonthlyLimitCountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return validationResponse;
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task<ValidationResponse> CheckMonthlyLimitAmountAsync(CheckLimitRequest request, List<MerchantLimit> activeMerchantLimits,
            MerchantMonthlyUsage activeMerchantMonthlyUsage)
        {
            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var monthlyLimitAmount = activeMerchantLimits
                .FirstOrDefault(x => x.Period == Period.Monthly
                                  && x.LimitType == LimitType.Amount
                                  && x.Currency == request.Currency
                                  && x.TransactionLimitType == transactionLimitType
                                  && x.RecordStatus == RecordStatus.Active);

            if (monthlyLimitAmount == null)
            {
                return new ValidationResponse { IsValid = true };
            }

            var current = activeMerchantMonthlyUsage.Amount + request.Amount;

            if (monthlyLimitAmount.MaxAmount < current)
            {
                var validationResponse =
                    await GetValidationResponseAsync(ApiErrorCode.MonthlyLimitAmountExceeded,
                        request.LanguageCode);

                _logger.LogError($"Provision PreValidation failed with code : {validationResponse.Code}, " +
                            $"Message: {validationResponse.Message}");

                await InsertValidationLogAsync(request, validationResponse);

                return validationResponse;
            }

            return new ValidationResponse
            {
                IsValid = true
            };
        }

        private async Task InsertValidationLogAsync(CheckLimitRequest request, ValidationResponse validationResponse)
        {
            await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                Amount = request.Amount,
                Currency = request.Currency,
                MerchantId = request.MerchantId,
                CardToken = request.CardToken,
                ConversationId = request.ConversationId,
                InstallmentCount = request.InstallmentCount,
                LanguageCode = request.LanguageCode,
                PointAmount = request.PointAmount,
                TransactionType = request.TransactionType,
                ClientIpAddress = request.ClientIpAddress,
                OriginalReferenceNumber = string.Empty,
                ThreeDSessionId = request.ThreeDSessionId,
                ErrorCode = validationResponse.Code,
                ErrorMessage = validationResponse.Message
            });
        }

        public async Task IncrementMerchantDailyUsageAsync(MerchantTransaction merchantTransaction)
        {
            if (merchantTransaction.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(merchantTransaction.TransactionType);

            var currency = await _currencyRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Number == merchantTransaction.Currency);

            var merchantDailyUsage = await _merchantDailyUsageRepository.GetAll()
            .FirstOrDefaultAsync(x => x.MerchantId == merchantTransaction.MerchantId
             && x.RecordStatus == RecordStatus.Active
             && x.Date == DateTime.Now.Date
             && x.Currency == currency.Code
             && x.TransactionLimitType == transactionLimitType);

            if (merchantDailyUsage is null)
            {
                merchantDailyUsage = new MerchantDailyUsage
                {
                    Count = 1,
                    Amount = merchantTransaction.Amount,
                    Date = DateTime.Now.Date,
                    MerchantId = merchantTransaction.MerchantId,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    Currency = currency.Code,
                    TransactionLimitType = transactionLimitType
                };

                await _merchantDailyUsageRepository.AddAsync(merchantDailyUsage);

                return;
            }

            merchantDailyUsage.Count += 1;
            merchantDailyUsage.Amount += merchantTransaction.Amount;

            await _merchantDailyUsageRepository.UpdateAsync(merchantDailyUsage);
        }

        public async Task IncrementMerchantMonthlyUsageAsync(MerchantTransaction merchantTransaction)
        {

            if (merchantTransaction.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(merchantTransaction.TransactionType);


            var currency = await _currencyRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Number == merchantTransaction.Currency);

            var merchantMonthlyUsage = await _merchantMonthlyUsageRepository.GetAll()
                .FirstOrDefaultAsync(x => x.MerchantId == merchantTransaction.MerchantId
                 && x.RecordStatus == RecordStatus.Active
                 && x.Date.Month == DateTime.Now.Month
                 && x.Date.Year == DateTime.Now.Year
                 && x.Currency == currency.Code
                 && x.TransactionLimitType == transactionLimitType);

            if (merchantMonthlyUsage is null)
            {
                merchantMonthlyUsage = new MerchantMonthlyUsage
                {
                    Count = 1,
                    Amount = merchantTransaction.Amount,
                    Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    MerchantId = merchantTransaction.MerchantId,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    Currency = currency.Code,
                    TransactionLimitType = transactionLimitType
                };

                await _merchantMonthlyUsageRepository.AddAsync(merchantMonthlyUsage);

                return;
            }

            merchantMonthlyUsage.Count += 1;
            merchantMonthlyUsage.Amount += merchantTransaction.Amount;

            await _merchantMonthlyUsageRepository.UpdateAsync(merchantMonthlyUsage);
        }

        public async Task IncrementSubMerchantDailyUsageAsync(MerchantTransaction merchantTransaction)
        {
            if (merchantTransaction.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(merchantTransaction.TransactionType);

            var currency = await _currencyRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Number == merchantTransaction.Currency);

            var subMerchantDailyUsage = await _subMerchantDailyUsageRepository.GetAll()
            .FirstOrDefaultAsync(x => x.SubMerchantId == merchantTransaction.SubMerchantId
             && x.RecordStatus == RecordStatus.Active
             && x.Date == DateTime.Now.Date
             && x.Currency == currency.Code
             && x.TransactionLimitType == transactionLimitType);

            if (subMerchantDailyUsage is null)
            {
                subMerchantDailyUsage = new SubMerchantDailyUsage
                {
                    Count = 1,
                    Amount = merchantTransaction.Amount,
                    Date = DateTime.Now.Date,
                    SubMerchantId = merchantTransaction.SubMerchantId.Value,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    Currency = currency.Code,
                    TransactionLimitType = transactionLimitType
                };

                await _subMerchantDailyUsageRepository.AddAsync(subMerchantDailyUsage);

                return;
            }

            subMerchantDailyUsage.Count += 1;
            subMerchantDailyUsage.Amount += merchantTransaction.Amount;

            await _subMerchantDailyUsageRepository.UpdateAsync(subMerchantDailyUsage);
        }

        public async Task IncrementSubMerchantMonthlyUsageAsync(MerchantTransaction merchantTransaction)
        {

            if (merchantTransaction.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(merchantTransaction.TransactionType);


            var currency = await _currencyRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Number == merchantTransaction.Currency);

            var subMerchantMonthlyUsage = await _subMerchantMonthlyUsageRepository.GetAll()
                .FirstOrDefaultAsync(x => x.SubMerchantId == merchantTransaction.SubMerchantId
                 && x.RecordStatus == RecordStatus.Active
                 && x.Date.Month == DateTime.Now.Month
                 && x.Date.Year == DateTime.Now.Year
                 && x.Currency == currency.Code
                 && x.TransactionLimitType == transactionLimitType);

            if (subMerchantMonthlyUsage is null)
            {
                subMerchantMonthlyUsage = new SubMerchantMonthlyUsage
                {
                    Count = 1,
                    Amount = merchantTransaction.Amount,
                    Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    SubMerchantId = merchantTransaction.SubMerchantId.Value,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    Currency = currency.Code,
                    TransactionLimitType = transactionLimitType
                };

                await _subMerchantMonthlyUsageRepository.AddAsync(subMerchantMonthlyUsage);

                return;
            }

            subMerchantMonthlyUsage.Count += 1;
            subMerchantMonthlyUsage.Amount += merchantTransaction.Amount;

            await _subMerchantMonthlyUsageRepository.UpdateAsync(subMerchantMonthlyUsage);
        }

        public async Task DecrementMerchantDailyUsageAsync(DecreaseMerchantLimitRequest request)
        {
            if (request.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var merchantDailyUsage = await _merchantDailyUsageRepository.GetAll()
                                                                        .FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId
                                                                         && x.RecordStatus == RecordStatus.Active
                                                                         && x.Date == request.TransactionDate.Date
                                                                         && x.TransactionLimitType == transactionLimitType);

            if (merchantDailyUsage is not null)
            {
                merchantDailyUsage.Count -= 1;
                merchantDailyUsage.Amount -= request.Amount;

                await _merchantDailyUsageRepository.UpdateAsync(merchantDailyUsage);
            }
        }

        public async Task DecrementMerchantMonthlyUsageAsync(DecreaseMerchantLimitRequest request)
        {
            if (request.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var merchantMonthlyUsage = await _merchantMonthlyUsageRepository.GetAll()
                                                                            .FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId
                                                                             && x.RecordStatus == RecordStatus.Active
                                                                             && x.Date.Month == DateTime.Now.Month
                                                                             && x.Date.Year == DateTime.Now.Year
                                                                             && x.TransactionLimitType == transactionLimitType);

            if (merchantMonthlyUsage is not null)
            {
                merchantMonthlyUsage.Count -= 1;
                merchantMonthlyUsage.Amount -= request.Amount;

                await _merchantMonthlyUsageRepository.UpdateAsync(merchantMonthlyUsage);
            }
        }

        public async Task DecrementSubMerchantDailyUsageAsync(DecreaseMerchantLimitRequest request)
        {
            if (request.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var subMerchantDailyUsage = await _subMerchantDailyUsageRepository.GetAll()
                                                                        .FirstOrDefaultAsync(x => x.SubMerchantId == request.SubMerchantId.Value
                                                                         && x.RecordStatus == RecordStatus.Active
                                                                         && x.Date == request.TransactionDate.Date
                                                                         && x.TransactionLimitType == transactionLimitType);

            if (subMerchantDailyUsage is not null)
            {
                subMerchantDailyUsage.Count -= 1;
                subMerchantDailyUsage.Amount -= request.Amount;

                await _subMerchantDailyUsageRepository.UpdateAsync(subMerchantDailyUsage);
            }
        }

        public async Task DecrementSubMerchantMonthlyUsageAsync(DecreaseMerchantLimitRequest request)
        {
            if (request.TransactionType is TransactionType.PostAuth or TransactionType.Reverse)
            {
                return;
            }

            var transactionLimitType = ConvertToTransactionLimitType(request.TransactionType);

            var subMerchantMonthlyUsage = await _subMerchantMonthlyUsageRepository.GetAll()
                                                                            .FirstOrDefaultAsync(x => x.SubMerchantId == request.SubMerchantId.Value
                                                                             && x.RecordStatus == RecordStatus.Active
                                                                             && x.Date.Month == DateTime.Now.Month
                                                                             && x.Date.Year == DateTime.Now.Year
                                                                             && x.TransactionLimitType == transactionLimitType);

            if (subMerchantMonthlyUsage is not null)
            {
                subMerchantMonthlyUsage.Count -= 1;
                subMerchantMonthlyUsage.Amount -= request.Amount;

                await _subMerchantMonthlyUsageRepository.UpdateAsync(subMerchantMonthlyUsage);
            }
        }

        private async Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode)
        {
            var merchantResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

            return new ValidationResponse
            {
                Code = merchantResponse.ResponseCode,
                IsValid = false,
                Message = merchantResponse.DisplayMessage
            };
        }

        private static TransactionLimitType ConvertToTransactionLimitType(TransactionType request)
        {
            return request switch
            {
                TransactionType.Auth => TransactionLimitType.Auth,
                TransactionType.PreAuth => TransactionLimitType.PreAuth,
                TransactionType.Return => TransactionLimitType.Return,
                _ => TransactionLimitType.Auth
            };
        }
    }
}
