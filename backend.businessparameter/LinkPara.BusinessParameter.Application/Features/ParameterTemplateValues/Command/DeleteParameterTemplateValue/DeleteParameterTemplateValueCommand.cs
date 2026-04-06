using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Command.DeleteParameterTemplateValue;
public class DeleteParameterTemplateValueCommand : IRequest
{
    [Audit]
    public Guid Id { get; set; }
}

public class DeleteParameterTemplateValueCommandHandler : IRequestHandler<DeleteParameterTemplateValueCommand>
{
    private readonly IParameterTemplateValueService _parameterTemplateValueService;

    public DeleteParameterTemplateValueCommandHandler(IParameterTemplateValueService parameterTemplateValueService)
    {
        _parameterTemplateValueService = parameterTemplateValueService;
    }
    public async Task<Unit> Handle(DeleteParameterTemplateValueCommand request, CancellationToken cancellationToken)
    {
        await _parameterTemplateValueService.DeleteAsync(request);

        return Unit.Value;
    }
}
