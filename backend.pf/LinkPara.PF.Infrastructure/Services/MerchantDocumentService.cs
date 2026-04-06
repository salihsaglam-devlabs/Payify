using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantDocuments.Commands.SaveMerchantDocument;
using LinkPara.PF.Application.Features.MerchantDocuments.Queries.GetMerchantDocumentsByTransactionIdQuery;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;


namespace LinkPara.PF.Infrastructure.Services
{
    public class MerchantDocumentService : IMerchantDocumentService
    {
        private readonly IGenericRepository<MerchantDocument> _merchantDocumentRepository;
        private readonly IMapper _mapper;

        public MerchantDocumentService(IGenericRepository<MerchantDocument> merchantDocumentRepository, IMapper mapper)
        {
            _merchantDocumentRepository = merchantDocumentRepository;
            _mapper = mapper;
        }

        public async Task<List<MerchantDocumentDto>> GetMerchantDocumentsByTransactionId(GetMerchantDocumentsByTransactionIdQuery query)
        {
            var merchantDocuments = await _merchantDocumentRepository.GetAll(x => x.MerchantTransactionId == query.Id).ToListAsync();

            return _mapper.Map<List<MerchantDocumentDto>>(merchantDocuments);
        }

        public async Task SaveMerchantDocumentsByTransactionId(SaveMerchantDocumentsByTransactionIdCommand command)
        {
        
            if (command.MerchantDocuments.Any())
            {
                foreach (var documentItem in command.MerchantDocuments)
                {
                    var merchantsDocument = await _merchantDocumentRepository
                        .GetAll().FirstOrDefaultAsync(x=> x.DocumentId == documentItem.DocumentId);

                    if (merchantsDocument != null && merchantsDocument.DocumentId != Guid.Empty)
                    {
                        merchantsDocument.DocumentId = documentItem.DocumentId;
                        merchantsDocument.DocumentTypeId = documentItem.DocumentTypeId;
                        merchantsDocument.DocumentName = documentItem.DocumentName;
                        merchantsDocument.MerchantId = command.MerchantId;
                        merchantsDocument.UpdateDate = DateTime.Now;
                        merchantsDocument.RecordStatus = documentItem.RecordStatus;

                        await _merchantDocumentRepository.UpdateAsync(merchantsDocument);
                    }
                    else
                    {
                        var merchantDocOb = new MerchantDocument()
                        {
                            DocumentId = documentItem.DocumentId,
                            DocumentTypeId = documentItem.DocumentTypeId,
                            DocumentName = documentItem.DocumentName,
                            MerchantId = command.MerchantId,
                            UpdateDate = DateTime.Now,
                            RecordStatus = documentItem.RecordStatus,
                            MerchantTransactionId = documentItem.MerchantTransactionId
                        };

                        await _merchantDocumentRepository.AddAsync(merchantDocOb);
                    }

                }
            }
        }
    }
}
