using AutoMapper;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.MainSubMerchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultiplePermission;

public class UpdateMultiplePermissionCommand : IRequest, IMapFrom<Merchant>
{
    public List<UpdateMultiplePermissionModel> MultiplePermissionModel { get; set; }
}
public class UpdateMultiplePermissionCommandHandler : IRequestHandler<UpdateMultiplePermissionCommand>
{
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILogger<UpdateMultiplePermissionCommandHandler> _logger;

    public UpdateMultiplePermissionCommandHandler(
        IGenericRepository<Merchant> merchantRepository,
        IContextProvider contextProvider,
        IMapper mapper,
        IAuditLogService auditLogService,
        ILogger<UpdateMultiplePermissionCommandHandler> logger,
        IStringLocalizerFactory factory)
    {
        _merchantRepository = merchantRepository;
        _logger = logger;
    }
    public async Task<Unit> Handle(UpdateMultiplePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in request.MultiplePermissionModel)
            {
                var merchant = await _merchantRepository.GetByIdAsync(item.MainSubMerchantId);

                if (merchant is null)
                {
                    throw new NotFoundException(nameof(Merchant), item.MainSubMerchantId);
                }

                merchant.FinancialTransactionAllowed = item.FinancialTransactionAllowed;
                merchant.PaymentAllowed = item.PaymentAllowed;
                merchant.IsCvvPaymentAllowed = item.IsCvvPaymentAllowed;
                merchant.IsReturnApproved = item.IsReturnApproved;
                merchant.PreAuthorizationAllowed = item.PreAuthorizationAllowed;
                merchant.PaymentReverseAllowed = item.PaymentReverseAllowed;
                merchant.PaymentReturnAllowed = item.PaymentReturnAllowed;
                merchant.InstallmentAllowed = item.InstallmentAllowed;
                merchant.Is3dRequired = item.Is3dRequired;
                merchant.IsExcessReturnAllowed = item.IsExcessReturnAllowed;
                merchant.InternationalCardAllowed = item.InternationalCardAllowed;
                merchant.IsPostAuthAmountHigherAllowed = item.IsPostAuthAmountHigherAllowed;

                await _merchantRepository.UpdateAsync(merchant);
            }

            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateMultiplePermissionCommandError : {exception}");
            throw;
        }
    }
}