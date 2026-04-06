using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantUsers.Command.SaveMerchantUser;

public class SaveMerchantUserCommand : IRequest, IMapFrom<MerchantUser>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public Guid MerchantId { get; set; }
}
public class SaveMerchantUserCommandHandler : IRequestHandler<SaveMerchantUserCommand>
{
    private readonly IMerchantUserService _merchantUserService;

    public SaveMerchantUserCommandHandler(IMerchantUserService merchantUserService)
    {
        _merchantUserService = merchantUserService;
    }
    public async Task<Unit> Handle(SaveMerchantUserCommand request, CancellationToken cancellationToken)
    {
        await _merchantUserService.SaveAsync(request);

        return Unit.Value;
    }
}
