using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class MerchantPoolHttpClient : HttpClientBase, IMerchantPoolHttpClient
{
    public MerchantPoolHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<ApproveMerchantPoolResponse> ApproveAsync(ApproveMerchantPoolServiceRequest request)
    {
        var response = await PutAsJsonAsync($"v1/MerchantPools", request);
        var approvePoolResponse = await response.Content.ReadFromJsonAsync<ApproveMerchantPoolResponse>();
        return approvePoolResponse ?? throw new InvalidOperationException();
    }

    public async Task<MerchantPoolDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/MerchantPools/{id}");
        var merchantPool = await response.Content.ReadFromJsonAsync<MerchantPoolDto>();

        if (!CanSeeSensitiveData())
        {
            merchantPool.Address = SensitiveDataHelper.MaskSensitiveData("Address", merchantPool.Address);
            merchantPool.Iban = SensitiveDataHelper.MaskSensitiveData("Iban", merchantPool.Iban);
            merchantPool.Email = SensitiveDataHelper.MaskSensitiveData("Email", merchantPool.Email);
            merchantPool.CompanyEmail = SensitiveDataHelper.MaskSensitiveData("Email", merchantPool.CompanyEmail);
            merchantPool.AuthorizedPersonIdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", merchantPool.AuthorizedPersonIdentityNumber);
            merchantPool.AuthorizedPersonName = SensitiveDataHelper.MaskSensitiveData("Name", merchantPool.AuthorizedPersonName);
            merchantPool.AuthorizedPersonCompanyPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", merchantPool.AuthorizedPersonCompanyPhoneNumber);
            merchantPool.AuthorizedPersonSurname = SensitiveDataHelper.MaskSensitiveData("Name", merchantPool.AuthorizedPersonSurname);
            merchantPool.AuthorizedPersonCompanyPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", merchantPool.AuthorizedPersonCompanyPhoneNumber);
            merchantPool.AuthorizedPersonMobilePhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", merchantPool.AuthorizedPersonMobilePhoneNumber);
            merchantPool.AuthorizedPersonMobilePhoneNumberSecond = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", merchantPool.AuthorizedPersonMobilePhoneNumberSecond);

        }

        return merchantPool ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<MerchantPoolDto>> GetFilterListAsync(GetFilterMerchantPoolRequest request)
    {
        var url = CreateUrlWithParams($"v1/MerchantPools", request, true);
        var response = await GetAsync(url);
        var merchantPools = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantPoolDto>>();

        if (!CanSeeSensitiveData())
        {
            merchantPools.Items.ForEach(s =>
            {
                s.Address = SensitiveDataHelper.MaskSensitiveData("Address", s.Address);
                s.Iban = SensitiveDataHelper.MaskSensitiveData("Iban", s.Iban);
                s.Email = SensitiveDataHelper.MaskSensitiveData("Email", s.Email);
                s.CompanyEmail = SensitiveDataHelper.MaskSensitiveData("Email", s.CompanyEmail);
                s.AuthorizedPersonIdentityNumber = SensitiveDataHelper.MaskSensitiveData("IdentityNumber", s.AuthorizedPersonIdentityNumber);
                s.AuthorizedPersonName = SensitiveDataHelper.MaskSensitiveData("Name", s.AuthorizedPersonName);
                s.AuthorizedPersonCompanyPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.AuthorizedPersonCompanyPhoneNumber);
                s.AuthorizedPersonSurname = SensitiveDataHelper.MaskSensitiveData("Name", s.AuthorizedPersonSurname);
                s.AuthorizedPersonCompanyPhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.AuthorizedPersonCompanyPhoneNumber);
                s.AuthorizedPersonMobilePhoneNumber = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.AuthorizedPersonMobilePhoneNumber);
                s.AuthorizedPersonMobilePhoneNumberSecond = SensitiveDataHelper.MaskSensitiveData("PhoneNumber", s.AuthorizedPersonMobilePhoneNumberSecond);
            });
        }

        return merchantPools ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(SaveMerchantPoolRequest request)
    {
        var response = await PostAsJsonAsync($"v1/MerchantPools", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
