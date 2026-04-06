using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Epin.Application.Features.Statistics.Queries;

public class GetReportQuery : IRequest<ReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetReportQueryHandler : IRequestHandler<GetReportQuery, ReportDto>
{
    private readonly IGenericRepository<Order> _orderRepository;

    public GetReportQueryHandler(IGenericRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ReportDto> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var reportDto = new ReportDto();

        var orders = await _orderRepository.GetAll()
            .Where(s =>
                s.CreateDate >= request.StartDate && s.CreateDate <= request.EndDate &&
                s.RecordStatus == RecordStatus.Active)
            .GroupBy(g => new { g.OrderStatus })
            .Select(s => new
            {
                Status = s.Key.OrderStatus,
                Count = s.Count(),
                Amount = s.Sum(k => k.UnitPrice)
            })
            .ToListAsync(cancellationToken: cancellationToken);

        if (orders.Any())
        {
            var succeeded = orders.FirstOrDefault(s => s.Status == OrderStatus.Completed);
            reportDto.Succeeded = succeeded?.Count ?? 0;
            reportDto.SucceededAmount = succeeded?.Amount ?? 0;

            var notSucceeded = orders.Where(s => s.Status != OrderStatus.Completed);
            foreach (var failed in notSucceeded)
            {
                reportDto.Failed += failed.Count;
                reportDto.FailedAmount += failed.Amount;
            }
        }

        // todo : donus bekleniyor
        reportDto.PendingReconciliation = 0;

        return reportDto;
    }
}