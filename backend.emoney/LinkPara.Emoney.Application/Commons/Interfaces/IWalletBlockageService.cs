using LinkPara.Emoney.Application.Features.WalletBlockages;
using LinkPara.Emoney.Application.Features.WalletBlockages.Commands;
using LinkPara.Emoney.Application.Features.WalletBlockages.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IWalletBlockageService
{
    Task<PaginatedList<WalletBlockageDto>> GetWalletBlockageAsync(GetWalletBlockageQuery request);
    Task AddWalletBlockageAsync(AddWalletBlockageCommand request);    
    Task<WalletBlockageDocumentDto> AddWalletBlockageDocumentAsync(AddWalletBlockageDocumentCommand request);
    Task<bool> RemoveWalletBlockageDocumentAsync(RemoveWalletBlockageDocumentCommand request);
    Task<List<WalletBlockageDocumentDto>> GetWalletBlockageDocumentsAsync(GetWalletBlockageDocumentQuery request);
    Task<List<WalletBlockage>> RemoveExpiredBlockagesAsync();
}
