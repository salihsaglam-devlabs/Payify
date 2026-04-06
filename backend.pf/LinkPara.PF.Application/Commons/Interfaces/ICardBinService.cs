using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Application.Features.CardBins.Command.DeleteCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.UpdateCardBin;
using LinkPara.PF.Application.Features.CardBins.Queries.GetAllCardBin;
using LinkPara.PF.Application.Features.CardBins.Queries.GetCardBinById;
using LinkPara.PF.Application.Features.Payments.Commands.GetBinInformation;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ICardBinService
{
    Task<PaginatedList<CardBinDto>> GetListAsync(GetAllCardBinQuery request);
    Task<CardBinDto> GetByNumberAsync(string binNumber);
    Task<CardBinDto> GetByIdAsync(GetCardBinByIdQuery request);
    Task SaveAsync(SaveCardBinCommand request);
    Task DeleteAsync(DeleteCardBinCommand request);
    Task UpdateAsync(UpdateCardBinCommand request);    
    Task<GetBinInformationResponse> GetBinInformationAsync(GetBinInformationCommand request);
}
