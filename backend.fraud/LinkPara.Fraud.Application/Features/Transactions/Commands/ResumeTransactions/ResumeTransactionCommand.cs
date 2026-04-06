using AutoMapper;
using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Domain.Entities;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.ResumeTransactions;

public class ResumeTransactionCommand : IRequest<ResumeRequest>
{
    public Guid TransactionId { get; set; }
}
public class ResumeTransactionCommandHandler : IRequestHandler<ResumeTransactionCommand, ResumeRequest>
{
    private readonly IGenericRepository<TransactionMonitoring> _repository;
    private readonly IFraudService _fraudService;

    public ResumeTransactionCommandHandler(IGenericRepository<TransactionMonitoring> repository,
        IFraudService fraudService)
    {
        _repository = repository;
        _fraudService = fraudService;
    }
    public async Task<ResumeRequest> Handle(ResumeTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == request.TransactionId);

        if (transaction is null)
        {
            throw new NotFoundException(nameof(TransactionMonitoring), request.TransactionId);
        }

        var resumeRequest = new ResumeRequest
        {
            CommandName = transaction.CommandName,
            Data = transaction.CommandJson,
            FraudId = transaction.Id
        };

        var result = await _fraudService.Resume(resumeRequest);
        if (result.Success == true)
        {
            transaction.MonitoringStatus = MonitoringStatus.Completed;
        }
        else
        {
            transaction.MonitoringStatus = MonitoringStatus.Failed;
            transaction.ErrorMessage = result.ErrorMessage;
        }
        await _repository.UpdateAsync(transaction);
        return resumeRequest;
    }
}