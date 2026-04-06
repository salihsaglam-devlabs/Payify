using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Commands.DeleteTransferOrder;

public class DeleteTransferOrderCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteTransferOrderCommandHandler : IRequestHandler<DeleteTransferOrderCommand>
{
    private readonly IGenericRepository<TransferOrder> _transferOrderRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public DeleteTransferOrderCommandHandler(
        IGenericRepository<TransferOrder> transferOrderRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _transferOrderRepository = transferOrderRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(DeleteTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var transferOrder = await _transferOrderRepository.GetByIdAsync(request.Id);

        if (transferOrder is null)
        {
            await TransferOrderAuditLogAsync(false, new Dictionary<string, string>
            {
                {"TransferOrderId",request.Id.ToString() },
                {"Message","NotFound"},
            });
            throw new NotFoundException(nameof(TransferOrder), request.Id);
        }

        if (!transferOrder.TransferOrderStatus.Equals(TransferOrderStatus.Pending))
        {
            await TransferOrderAuditLogAsync(false, new Dictionary<string, string>
            {
                {"TransferOrderId",transferOrder.Id.ToString() },
                {"Message","FieldAccess TransferOrderStatus Is Pending"},
            });
            throw new InvalidParameterException(nameof(transferOrder.TransferOrderStatus));
        }

        if(transferOrder.UserId != GetUserId())
        {
            await TransferOrderAuditLogAsync(false, new Dictionary<string, string>
            {
                {"TransferOrderId",transferOrder.Id.ToString() },
                {"TransferOrderUserId",transferOrder.UserId.ToString() },
                {"Message","UnAuthorizedAccess"},
            });
            throw new ForbiddenAccessException();
        }

        transferOrder.TransferOrderStatus = TransferOrderStatus.Canceled;
        await _transferOrderRepository.UpdateAsync(transferOrder);

        await TransferOrderAuditLogAsync(true, new Dictionary<string, string>
        {
            {"TransferOrderId",transferOrder.Id.ToString() },
            {"SenderWalletNumber",transferOrder.SenderWalletNumber },
            {"ReceiverNameSurname",transferOrder.ReceiverNameSurname }
        });

        return Unit.Value;
    }

    private async Task TransferOrderAuditLogAsync(bool isSuccess, Dictionary<string, string> deatils)
    {
        var userId = GetUserId();
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "DeleteTransferOrder",
                Resource = "TransferOrder",
                SourceApplication = "Emoney",
                UserId = userId
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