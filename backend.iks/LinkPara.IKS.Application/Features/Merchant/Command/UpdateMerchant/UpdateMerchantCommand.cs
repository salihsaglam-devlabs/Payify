using AutoMapper;
using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response;
using LinkPara.IKS.Application.Enums;
using MediatR;
using MainSellerFlag = LinkPara.IKS.Application.Enums.MainSellerFlag;


namespace LinkPara.IKS.Application.Features.Merchant.Command.UpdateMerchant
{
    public class UpdateMerchantCommand : IRequest<IKSResponse<MerchantResponse>>
    {
        public string MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TaxNo { get; set; }
        public string TradeName { get; set; }
        public string MerchantName { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        /// <summary>
        /// Semt
        /// </summary>
        public string Neighborhood { get; set; }
        /// <summary>
        /// Plaka kodu
        /// </summary>
        public int LicenseTag { get; set; }
        public string CountryCode { get; set; }
        /// <summary>
        /// isyerinin bulundugu ulke numerik ISO kodu
        /// </summary>
        public int Mcc { get; set; }
        public string ManagerName { get; set; }
        public string Phone { get; set; }
        public string ZipCode { get; set; }
        public string TaxOfficeName { get; set; }
        public string CommercialType { get; set; }
        public string StatusCode { get; set; }
        public MainSellerFlag MainSellerFlag { get; set; }
        public string MainSellerTaxNo { get; set; }
        public string TerminationCode { get; set; }
        public string EstablishmentDate { get; set; }
        public string BusinessModel { get; set; }
        public string BusinessActivity { get; set; }
        public int BranchCount { get; set; }
        public int EmployeeCount { get; set; }
        public string ManagerBirthDate { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public List<MerchantBusinessPartnerRequest> MerchantBusinessPartners { get; set; }
    }
    public class UpdateMerchantCommandHandler : IRequestHandler<UpdateMerchantCommand, IKSResponse<MerchantResponse>>
    {
        private readonly IIKSService _iKSService;
        private readonly IMapper _mapper;

        public UpdateMerchantCommandHandler(IIKSService iKSService,
            IMapper mapper)
        {
            _iKSService = iKSService;
            _mapper = mapper;
        }

        public async Task<IKSResponse<MerchantResponse>> Handle(UpdateMerchantCommand request, CancellationToken cancellationToken)
        {
            var merchantMap = _mapper.Map<UpdateMerchantRequest>(request);
            merchantMap.PspFlag = ((int)PspFlag.PspFlag).ToString();
            merchantMap.MainSellerFlag = ((int)request.MainSellerFlag).ToString();
            merchantMap.AgreementDate = DateTime.Now.ToString("dd.MM.yyyy");
            merchantMap.Partners = request.MerchantBusinessPartners;

            var result = await _iKSService.UpdateMerchantAsync(merchantMap);

            var data = _mapper.Map<MerchantResponse>(result?.Data?.merchant);

            return new IKSResponse<MerchantResponse>()
            {
                Error = result.Error,
                Data = data,
                StatusCode = result.StatusCode
            };
        }
    }
}
