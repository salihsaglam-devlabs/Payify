using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.DeleteMcc;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.Links.Command.DeleteLink;

public class DeleteLinkCommand : IRequest
{
    public Guid Id { get; set; }
}
public class DeleteLinkCommandHandler : IRequestHandler<DeleteLinkCommand>
{
    private readonly ILinkService _linkService;

    public DeleteLinkCommandHandler(ILinkService linkService)
    {
        _linkService = linkService;
    }

    public async Task<Unit> Handle(DeleteLinkCommand request, CancellationToken cancellationToken)
    {
        await _linkService.DeleteAsync(request);

        return Unit.Value;
    }
}
