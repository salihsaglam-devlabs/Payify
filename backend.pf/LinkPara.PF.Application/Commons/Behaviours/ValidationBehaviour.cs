using System.Text;
using FluentValidation;
using FluentValidation.Results;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Application.Features.Payments.Commands.Reverse;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using ValidationException = LinkPara.SharedModels.Exceptions.ValidationException;

namespace LinkPara.PF.Application.Commons.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IApplicationUserService _applicationUserService;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators, 
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository, 
        IApplicationUserService applicationUserService)
    {
        _validators = validators;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _applicationUserService = applicationUserService;
    }
    
    public async Task<TResponse> Handle(TRequest request,RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            
            if (failures.Any())
            {
                if (typeof(TRequest).GetInterface(nameof(IClientApiCommand)) != null)
                {
                    await SaveClientApiValidationLogAsync(request, failures);
                }

                throw new ValidationException(failures);
            }
        }
        return await next();
    }

    private async Task SaveClientApiValidationLogAsync(TRequest request, List<ValidationFailure> validationFailures)
    {
        var errorMessageBuilder = new StringBuilder();
        foreach (var failure in validationFailures)
        {
            errorMessageBuilder.Append(failure.ErrorMessage).Append(' ');
        }
        
        if(request is ProvisionCommand command)
        {
            await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                Amount = command.Amount,
                Currency = command.Currency,
                MerchantId = command.MerchantId,
                CardToken = command.CardToken,
                ConversationId = command.ConversationId,
                InstallmentCount = command.InstallmentCount,
                LanguageCode = command.LanguageCode,
                PointAmount = command.PointAmount,
                TransactionType = command.PaymentType switch
                {
                    VposPaymentType.Auth => TransactionType.Auth,
                    VposPaymentType.PreAuth => TransactionType.PreAuth,
                    _ => TransactionType.PostAuth
                },
                ClientIpAddress = command.ClientIpAddress,
                OriginalReferenceNumber = command.OriginalOrderId,
                ThreeDSessionId = command.ThreeDSessionId,
                // todo : Gökhandan gelecek hatalara göre belirlenecek
                ErrorCode = "501",
                ErrorMessage = errorMessageBuilder.ToString()
            });
        }
        else if (request is ReturnCommand returnCommand)
        {
            await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                Amount = returnCommand.Amount,
                Currency = string.Empty,
                MerchantId = returnCommand.MerchantId,
                CardToken = string.Empty,
                ConversationId = returnCommand.ConversationId,
                InstallmentCount = 1,
                LanguageCode = returnCommand.LanguageCode,
                PointAmount = 0,
                TransactionType = TransactionType.Return,
                ClientIpAddress = returnCommand.ClientIpAddress,
                OriginalReferenceNumber = returnCommand.OrderId,
                ThreeDSessionId = string.Empty,
                ErrorCode = "501",
                ErrorMessage = errorMessageBuilder.ToString()
            });
        }
        else if (request is ReverseCommand reverseCommand)
        {
            await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                Amount = 0,
                Currency = string.Empty,
                MerchantId = reverseCommand.MerchantId,
                CardToken = string.Empty,
                ConversationId = reverseCommand.ConversationId,
                InstallmentCount = 1,
                LanguageCode = reverseCommand.LanguageCode,
                PointAmount = 0,
                TransactionType = TransactionType.Reverse,
                ClientIpAddress = reverseCommand.ClientIpAddress,
                OriginalReferenceNumber = reverseCommand.OrderId,
                ThreeDSessionId = string.Empty,
                ErrorCode = "501",
                ErrorMessage = errorMessageBuilder.ToString()
            });
        }
    }
}