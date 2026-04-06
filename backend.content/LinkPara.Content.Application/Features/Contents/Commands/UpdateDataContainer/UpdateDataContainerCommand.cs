using System.Text.Json;
using LinkPara.Content.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Content.Application.Features.Contents.Commands.UpdateDataContainer;

public class UpdateDataContainerCommand : IRequest
{
    public string Key { get; set; }
    public JsonElement Value { get; set; }
}


public class UpdateDataContainerCommandHandler : IRequestHandler<UpdateDataContainerCommand>
{
    private readonly IGenericRepository<DataContainer> _dataContainerRepository;

    public UpdateDataContainerCommandHandler(IGenericRepository<DataContainer> dataContainerRepository)
    {
        _dataContainerRepository = dataContainerRepository;
    }

    public async Task<Unit> Handle(UpdateDataContainerCommand request, CancellationToken cancellationToken)
    {
        var dataContainer = _dataContainerRepository.GetAll().SingleOrDefault(x => x.Key == request.Key);

        if (dataContainer is not null)
        {
            dataContainer.Value = request.Value.ToString();

            await _dataContainerRepository.UpdateAsync(dataContainer);
        }
        return Unit.Value;
    }
}
