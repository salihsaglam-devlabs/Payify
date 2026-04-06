using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LinkPara.PF.Application.Features.Payments.Commands.Verify3ds;

public class Verify3dsCommand : IRequest<Verify3dsResponse>
{
    public string OrderId { get; set; }
    public Guid ThreedSessionId { get; set; }
    public Dictionary<string,string> FormCollection { get; set; }
}

public class Verify3dsCommandHandler : IRequestHandler<Verify3dsCommand, Verify3dsResponse>
{
    private readonly IThreeDService _threeDService;

    public Verify3dsCommandHandler(IThreeDService threeDService)
    {
        _threeDService = threeDService;
    }

    public async Task<Verify3dsResponse> Handle(Verify3dsCommand request, CancellationToken cancellationToken)
    {
        return await _threeDService.Verify3ds(request);
    }
}