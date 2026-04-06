using LinkPara.Content.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Content.Application.Features.Contents.Queries
{
    public class GetDataContainerQuery : IRequest<DataContainer>
    {        
        public string Key { get; set; }
    }

    public class GetDataContainerQueryHandler : IRequestHandler<GetDataContainerQuery, DataContainer>
    {
        private readonly IGenericRepository<DataContainer> _dataContainerRepository;

        public GetDataContainerQueryHandler(IGenericRepository<DataContainer> dataContainerRepository)
        {
            _dataContainerRepository = dataContainerRepository;
        }
        public async Task<DataContainer> Handle(GetDataContainerQuery request, CancellationToken cancellationToken)
        {
            return await _dataContainerRepository.GetAll()
                .SingleOrDefaultAsync(x => x.Key == request.Key, cancellationToken);
        }
    }
}
