using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Identity;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountByIdQuery;

public class GetAccountByIdQuery : IRequest<AccountDto>
{
    public Guid Id { get; set; }
}

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IGenericRepository<Account> _repository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<GetAccountByIdQueryHandler> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IUserActivityLogService _userActivityLogService;

    public GetAccountByIdQueryHandler(
        IGenericRepository<Account> repository,
        IMapper mapper,
        IUserService userService,
        ILogger<GetAccountByIdQueryHandler> logger,
        IUserActivityLogService userActivityLogService,
        IContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
        _userActivityLogService = userActivityLogService;
        _repository = repository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository
            .GetAll()
            .Include(s => s.AccountUsers)
            .FirstOrDefaultAsync(s => s.Id == request.Id);

        NullControlHelper.CheckAndThrowIfNull(account, request.Id, _logger);

        if (account.RecordStatus == RecordStatus.Passive
            && !string.IsNullOrEmpty(account.IdentityNumber))
        {
            Regex identityNumber = new Regex(@"(?<!\d)\d{11}(?!\d)");
            var value = identityNumber.Match(account.IdentityNumber);
            account.IdentityNumber = value.Success ? value.Value : account.IdentityNumber;
        }

        try
        {
            var user = await _userService.GetUserAsync(account.AccountUsers[0].UserId);

            if (!user.BirthDate.Equals(account.BirthDate))
            {
                account.BirthDate = user.BirthDate;

                await _repository.UpdateAsync(account);
            }
            await _userActivityLogService.UserActivityLogAsync(
                            new UserActivityLog
                            {
                                LogDate = DateTime.Now,
                                Operation = "UserOperations/Detail",
                                Resource = "Accounts",
                                SourceApplication = "Emoney",
                                ViewerId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                                : Guid.Empty,
                                ViewedId = account.AccountUsers[0].UserId
                            }

                        );
        }
        catch (Exception e)
        {
            _logger.LogError($"GetAccountByIdQueryHandlerError: {e}");
        }

        return _mapper.Map<AccountDto>(account);
    }
}
