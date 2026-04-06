using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LinkPara.Emoney.Application.Features.Chargebacks.Commands;

public class AddChargebackDocumentCommand : IRequest<ChargebackDocumentDto>
{
    public Guid ChargebackId { get; set; }
    public string DocumentDescription { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string OriginalFileName { get; set; }
    public Guid DocumentTypeId { get; set; }
}

public class AddChargebackDocumentCommandHandler : IRequestHandler<AddChargebackDocumentCommand, ChargebackDocumentDto>
{
    
    private readonly IChargebackService _chargebackService;

    public AddChargebackDocumentCommandHandler(IChargebackService chargebackService)
    {
        _chargebackService = chargebackService;
    }

    public async Task<ChargebackDocumentDto> Handle(AddChargebackDocumentCommand request, CancellationToken cancellationToken)
    {
        return await _chargebackService.AddChargebackDocumentAsync(request);        
    }
}