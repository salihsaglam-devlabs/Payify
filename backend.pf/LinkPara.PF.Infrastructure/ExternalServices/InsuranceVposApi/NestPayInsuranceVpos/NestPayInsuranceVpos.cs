using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.NestPayInsuranceVpos
{
    public class NestPayInsuranceVpos : VposBase, IVposApi
    {
        public Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PosRefundResponse> Refund(PosRefundRequest request)
        {
            throw new NotImplementedException();
        }

        public void SetServiceParameters(object serviceParameters, Guid? merchantId = null)
        {
            throw new NotImplementedException();
        }

        public Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string, string> form)
        {
            throw new NotImplementedException();
        }

        public Task<PosVoidResponse> Void(PosVoidRequest request)
        {
            throw new NotImplementedException();
        }

        protected override string FormatAmount(decimal amount)
        {
            throw new NotImplementedException();
        }

        protected override string FormatExpiryDate(string month, string year)
        {
            throw new NotImplementedException();
        }
    }
}
