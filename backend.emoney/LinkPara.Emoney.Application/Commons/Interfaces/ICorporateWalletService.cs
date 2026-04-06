using LinkPara.Emoney.Application.Features.CompanyPools.Commands.ApproveCompanyPool;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.AddUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeactivateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeleteUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateUser;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ICorporateWalletService
{
    Task ActionCompanyPoolAsync(ApproveCompanyPoolCommand request);
    Task ActivateCorporateAccountAsync(ActivateCorporateAccountCommand request);
    Task ActivateCorporateWalletUserAsync(ActivateCorporateWalletUserCommand request);
    Task AddCorporateWalletUserAsync(AddCorporateWalletUserCommand request);
    Task DeactivateCorporateAccountAsync(DeactivateCorporateAccountCommand request);
    Task DeactivateCorporateWalletUserAsync(DeactivateCorporateWalletUserCommand request);
    Task UpdateCorporateAccountAsync(UpdateCorporateAccountCommand updateRequest);
    Task UpdateCorporateWalletUserAsync(UpdateCorporateWalletUserCommand request);
}
