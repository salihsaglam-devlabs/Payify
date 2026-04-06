using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Billing.Application.Features.Institutions.Commands;

public class UpdateInstitutionCommand : IRequest
{
    public Guid InstitutionId { get; set; }
    public Guid ActiveVendorId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class SaveInstitutionCommandHandler : IRequestHandler<UpdateInstitutionCommand>
{
    private readonly IInstitutionService _institutionService;

    public SaveInstitutionCommandHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<Unit> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        await _institutionService.UpdateAsync(request);

        return Unit.Value;
    }
}
