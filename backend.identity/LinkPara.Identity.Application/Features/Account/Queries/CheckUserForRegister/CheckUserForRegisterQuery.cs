using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Account.Commands.Register;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LinkPara.Identity.Application.Features.Account.Queries.CheckUserInformation
{
    public class CheckUserForRegisterQuery : IRequest<bool>
    {
        public string UserName { get; set; }
        public string Msisdn { get; set; }
        public string Tckn { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Nationality { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public Guid? ParentAccountId { get; set; }
        public UserType UserType { get; set; }

        public List<Document> Documents { get; set; }

        public class Document
        {
            public string Id { get; set; }
            public bool IsAccepted { get; set; }
        }
    }

    public class CheckUserInformationQueryHandler : IRequestHandler<CheckUserForRegisterQuery, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IParameterService _parameterService;
        private readonly IStringLocalizer _localizer;
        private readonly ISearchService _searchService;
        private readonly IRepository<AgreementDocumentVersion> _agreementDocumentVersionRepository;
        private const int MatchRate = 90;

        public CheckUserInformationQueryHandler(IRepository<User> userRepository, IParameterService parameterService,
            IStringLocalizerFactory factory, ISearchService searchService, IRepository<AgreementDocumentVersion> agreementDocumentRepository)
        {
            _userRepository = userRepository;
            _parameterService = parameterService;
            _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
            _searchService = searchService;
            _agreementDocumentVersionRepository = agreementDocumentRepository;
        }

        public async Task<bool> Handle(CheckUserForRegisterQuery request, CancellationToken cancellationToken)
        {

            var phoneUser = await _userRepository
             .GetAll()
             .FirstOrDefaultAsync(s =>
                 (s.UserName == request.UserName &&
                 (s.UserStatus == UserStatus.Active || s.UserStatus == UserStatus.Suspended))
                 || (s.IdentityNumber == request.Tckn && s.UserType == request.UserType));

            if (phoneUser is not null)
            {
                throw new AlreadyInUseException(nameof(request.Msisdn));
            }
            await IsBirthdateBetweenAllowedRangeAsync(request.BirthDate, (request.ParentAccountId != Guid.Empty));

            var isBlacklistCheckEnabled =
           (await _parameterService.GetParameterAsync("BlackListParameters", "CheckAtRegister")).ParameterValue == "1";

            if (isBlacklistCheckEnabled)
            {
                await CheckBlackListAsync(request);
            }

            await CheckAgreementDocuments(request.Documents);

            return true;
        }

        private async Task CheckAgreementDocuments(List<CheckUserForRegisterQuery.Document> documents)
        {
            var documentVersionList = _agreementDocumentVersionRepository
                  .GetAll(s => s.AgreementDocument)
                  .Where(q => q.IsLatest &&
                q.RecordStatus == RecordStatus.Active &&
                q.AgreementDocument != null &&
                q.AgreementDocument.ProductType == ProductType.Emoney);

            if (!documentVersionList.Any())
            {
                throw new InvalidOperationException();
            }

            foreach (var item in documents)
            {
                var documentVersion = documentVersionList.FirstOrDefault(x => x.AgreementDocumentId == Guid.Parse(item.Id));
                if (documentVersion == null)
                {
                    throw new NotFoundException("Not found agreement document ıd", item.Id);
                }

                if (documentVersion != null && !documentVersion.IsOptional && !item.IsAccepted)
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                        {
                            { "DocumentId", new[] { $"Document with ID {documentVersion.Id} must be accepted." } }
                        });
                }
            }

        }

        private async Task IsBirthdateBetweenAllowedRangeAsync(DateTime dt, bool isChildAccount)
        {
            DateTime rangeStart, rangeEnd;
            var customerAgeRequirements = await _parameterService.GetParametersAsync("CustomerAgeRequirements");

            if (!isChildAccount)
            {
                _ = int.TryParse(
                    customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue, out var minAge);
                rangeStart = DateTime.Now.AddYears(-1 * minAge);

                _ = int.TryParse(
                    customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MaxAge")?.ParameterValue, out var maxAge);
                rangeEnd = DateTime.Now.AddYears(-1 * maxAge);

                if (!(dt <= rangeStart && dt >= rangeEnd))
                {
                    var exceptionMessage = _localizer.GetString("BirthdateOutOfRange")
                        .Value.Replace("@@minAge", minAge.ToString());

                    throw new BirthdateOutOfRangeException(exceptionMessage);
                }
            }
            else
            {
                _ = int.TryParse(
                    customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinChildAge")?.ParameterValue, out var minAge);
                rangeStart = DateTime.Now.AddYears(-1 * minAge);

                _ = int.TryParse(
                   customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue, out var maxAge);
                rangeEnd = DateTime.Now.AddYears(-1 * maxAge);

                if (!(dt <= rangeStart && dt >= rangeEnd))
                {
                    var exceptionMessage = _localizer.GetString("ChildAccountBirthdateOutOfRange").Value
                        .Replace("@@minAge", minAge.ToString())
                        .Replace("@@maxAge", maxAge.ToString());

                    throw new BirthdateOutOfRangeException(exceptionMessage);
                }
            }
        }

        private async Task CheckBlackListAsync(CheckUserForRegisterQuery request)
        {
            SearchByNameRequest searchRequest = new()
            {
                Name = $"{request.Name} {request.Surname}",
                BirthYear = request.BirthDate.Year.ToString(),
                SearchType = SearchType.Any,
                FraudChannelType = FraudChannelType.Web
            };
            var blackListControl = await _searchService.GetSearchByName(searchRequest);
            if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= MatchRate)
            {
                var informationMail = await _parameterService.GetParameterAsync("CompanyContactInformation", "CompanyEmail");

                var exceptionMessage = _localizer.GetString("UserInBlacklistException");

                throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
            }
        }
    }
}
