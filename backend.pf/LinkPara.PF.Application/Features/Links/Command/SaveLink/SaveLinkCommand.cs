using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;


namespace LinkPara.PF.Application.Features.Links.Command.SaveLink;

public class SaveLinkCommand : IRequest<LinkResponse>
{
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public LinkType LinkType { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int MaxUsageCount { get; set; }
    public string OrderId { get; set; }
    public LinkAmountType LinkAmountType { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
    public List<int> Installments { get; set; }
    public bool CommissionFromCustomer { get; set; }
    public bool Is3dRequired { get; set; }
    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public string ReturnUrl { get; set; }
    public bool IsNameRequired { get; set; }
    public bool IsEmailRequired { get; set; }
    public bool IsPhoneNumberRequired { get; set; }
    public bool IsAddressRequired { get; set; }
    public bool IsNoteRequired { get; set; }
}
public class SaveLinkCommandHandler : IRequestHandler<SaveLinkCommand,LinkResponse>
{
    private readonly ILinkService _linkService;
    public SaveLinkCommandHandler(ILinkService linkService)
    {
        _linkService = linkService;
    }
    public async Task<LinkResponse> Handle(SaveLinkCommand request, CancellationToken cancellationToken)
    {
       var linkResponse = await _linkService.SaveAsync(request);

        return linkResponse;
    }
}