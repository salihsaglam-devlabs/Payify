using LinkPara.BusinessParameter.Application.Commons.Attributes;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.DeleteParameterTemplate;
public class DeleteParameterTemplateCommand : IRequest
{
    [Audit]
    public Guid Id { get; set; }
}

public class DeleteParameterTemplateCommandHandler : IRequestHandler<DeleteParameterTemplateCommand>
{
    private readonly IParameterTemplateService _parameterTemplateService;

    public DeleteParameterTemplateCommandHandler(IParameterTemplateService parameterTemplateService)
    {
        _parameterTemplateService = parameterTemplateService;
    }
    public async Task<Unit> Handle(DeleteParameterTemplateCommand request, CancellationToken cancellationToken)
    {
        await _parameterTemplateService.DeleteAsync(request);

        return Unit.Value;
    }
}
