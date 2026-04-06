using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Command.DeleteParameter;
public class DeleteParameterCommand : IRequest
{
    [Audit]
    public Guid Id { get; set; }
}

public class DeleteParameterCommandHandler : IRequestHandler<DeleteParameterCommand>
{
    private readonly IParameterService _parameterService;

    public DeleteParameterCommandHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }
    public async Task<Unit> Handle(DeleteParameterCommand request, CancellationToken cancellationToken)
    {
        await _parameterService.DeleteAsync(request);

        return Unit.Value;
    }
}
