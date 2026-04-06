using LinkPara.Billing.Application.Commons.Attributes;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Billing.Application.Features.Vendors.Commands;

public class SaveVendorCommand : IRequest
{
    [Audit]
    public string Name { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class SaveVendorCommandHandler : IRequestHandler<SaveVendorCommand>
{
    private readonly IVendorService _vendorService;

    public SaveVendorCommandHandler(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    public async Task<Unit> Handle(SaveVendorCommand request, CancellationToken cancellationToken)
    {
        await _vendorService.SaveAsync(request);
        return Unit.Value;
    }
}