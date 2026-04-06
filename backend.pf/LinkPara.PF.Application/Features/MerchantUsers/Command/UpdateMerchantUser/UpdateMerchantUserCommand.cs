using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantUsers.Command.UpdateMerchantUser;

public class UpdateMerchantUserCommand : IRequest, IMapFrom<MerchantUser>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class UpdateMerchantUserCommandHandler : IRequestHandler<UpdateMerchantUserCommand>
{
    private readonly IMerchantUserService _merchantUserService;

    public UpdateMerchantUserCommandHandler(IMerchantUserService merchantUserService)
    {
        _merchantUserService = merchantUserService;
    }

    public async Task<Unit> Handle(UpdateMerchantUserCommand request, CancellationToken cancellationToken)
    {
        await _merchantUserService.UpdateAsync(request);

        return Unit.Value;
    }
}
