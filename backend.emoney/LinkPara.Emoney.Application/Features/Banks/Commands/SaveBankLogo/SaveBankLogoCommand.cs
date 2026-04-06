using LinkPara.Emoney.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkPara.Emoney.Application.Features.Banks.Queries;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;

namespace LinkPara.Emoney.Application.Features.Banks.Commands.SaveBankLogo;
public class SaveBankLogoCommand : IRequest
{
    public BankLogoDto BankLogo { get; set; }
}

public class SaveBankLogoCommandHandler : IRequestHandler<SaveBankLogoCommand>
{
    private readonly IGenericRepository<BankLogo> _bankLogoRepository;
    private readonly IApplicationUserService _applicationUserService;

    public SaveBankLogoCommandHandler(IGenericRepository<BankLogo> bankLogoRepository, 
        IApplicationUserService applicationUserService)
    {
        _bankLogoRepository = bankLogoRepository;
        _applicationUserService = applicationUserService;
    }

    public async Task<Unit> Handle(SaveBankLogoCommand command, CancellationToken cancellationToken)
    {
        var bankLogo = await _bankLogoRepository.GetAll()
            .FirstOrDefaultAsync(x => x.BankId == command.BankLogo.BankId, cancellationToken);

        if (bankLogo is not null)
        {
            bankLogo.Bytes = command.BankLogo.Bytes;
            bankLogo.ContentType = command.BankLogo.ContentType;

            await _bankLogoRepository.UpdateAsync(bankLogo);
        }

        else
        {
            bankLogo = new BankLogo()
            {
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                BankId = command.BankLogo.BankId,
                Bytes = command.BankLogo.Bytes,
                ContentType = command.BankLogo.ContentType
            };

            await _bankLogoRepository.AddAsync(bankLogo);
        }

        return Unit.Value;
    }
}