using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Models.CompanyPoolModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Documents.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Commands.SaveCompanyPool;

public class SaveCompanyPoolCommand : IRequest<SaveCompanyPoolResponse>
{
    public CompanyType CompanyType { get; set; }
    public string Title { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string LandPhone { get; set; }
    public string WebSiteUrl { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public string Iban { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
    public string AuthorizedPersonIdentityNumber { get; set; }
    public string AuthorizedPersonName { get; set; }
    public string AuthorizedPersonSurname { get; set; }
    public DateTime AuthorizedPersonBirthDate { get; set; }
    public string AuthorizedPersonCompanyPhoneCode { get; set; }
    public string AuthorizedPersonCompanyPhoneNumber { get; set; }
    public string AuthorizedPersonEmail { get; set; }
    public List<CompanyPoolDocument> Documents { get; set; }
    public string MersisNumber { get; set; }
    public CompanyPoolChannel Channel { get; set; }
}

public class SaveCompanyPoolCommandHandler : IRequestHandler<SaveCompanyPoolCommand, SaveCompanyPoolResponse>
{
    private readonly IGenericRepository<CompanyPool> _repository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IDocumentService _documentService;
    private readonly IParameterService _parameterService; 
    private readonly IStringLocalizer _documentTypesLocalizer;
    private readonly IStringLocalizer _exceptionsLocalizer;
    private readonly IContextProvider _contextProvider;

    public SaveCompanyPoolCommandHandler(IGenericRepository<CompanyPool> repository,
        IApplicationUserService applicationUserService,
        IDocumentService documentService,
        IParameterService parameterService,
        IStringLocalizerFactory factory,
        IContextProvider contextProvider)
    {
        _repository = repository;
        _applicationUserService = applicationUserService;
        _documentService = documentService;
        _parameterService = parameterService;
        _documentTypesLocalizer = factory.Create("DocumentTypes", "LinkPara.Emoney.API");
        _exceptionsLocalizer = factory.Create("Exceptions", "LinkPara.Emoney.API");
        _contextProvider = contextProvider;
    }

    public async Task<SaveCompanyPoolResponse> Handle(SaveCompanyPoolCommand request, CancellationToken cancellationToken)
    {
        await ValidateCompanyPool(request);


        var newCompany = PrepareCompanyPool(request);

        await CheckRequiredDocumentsAsync(newCompany, request.Documents);

        CheckDuplicateDocuments(request);

        await AddDocumentsAsync(newCompany.Id, request.Documents);

        await _repository.AddAsync(newCompany);

        return new SaveCompanyPoolResponse { CompanyPoolId = newCompany.Id };
    }

    private static void CheckDuplicateDocuments(SaveCompanyPoolCommand request)
    {
        var duplicateDocuments = request.Documents.GroupBy(d => new { d.File.FileName, d.File.ContentType })
                                  .Where(g => g.Count() > 1)
                                  .Select(g => g.Key)
                                  .ToList();

        if (duplicateDocuments.Any())
        {
            throw new DuplicateDocumentException();
        }
    }

    private async Task CheckRequiredDocumentsAsync(CompanyPool companyPool, List<CompanyPoolDocument> documents)
    {
        var parameters = await _parameterService.GetParametersAsync("CorporateWalletDocumentType");

        if (parameters is null)
        {
            return;
        }

        var documentTypes = new List<CompanyDocumentTypeDto>();

        foreach (var parameter in parameters)
        {
            var templates = await _parameterService.GetAllParameterTemplateValuesAsync(parameter.GroupCode, parameter.ParameterCode);

            var companyDocumentType = new CompanyDocumentTypeDto
            {
                Name = parameter.ParameterCode,
                DocumentTypeId = Guid.Parse(parameter.ParameterValue)
            };

            if (companyPool.CompanyType == CompanyType.Individual)
            {
                if (templates.Any(x => x.TemplateCode == "IsIndividualRequired"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "IsIndividualRequired");

                    if (bool.TryParse(template.TemplateValue, out bool isRequired) && isRequired)
                    {
                        companyDocumentType.IsRequired = true;
                    }
                }

                if (templates.Any(x => x.TemplateCode == "HasIndividualDocument"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "HasIndividualDocument");

                    if (bool.TryParse(template.TemplateValue, out bool hasIndividualDocument) && hasIndividualDocument)
                    {
                        documentTypes.Add(companyDocumentType);
                    }
                }
            }

            if (companyPool.CompanyType == CompanyType.Corporate)
            {
                if (templates.Any(x => x.TemplateCode == "IsCorporateRequired"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "IsCorporateRequired");

                    if (bool.TryParse(template.TemplateValue, out bool isRequired) && isRequired)
                    {
                        companyDocumentType.IsRequired = true;
                    }
                }

                if (templates.Any(x => x.TemplateCode == "HasCorporateDocument"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "HasCorporateDocument");

                    if (bool.TryParse(template.TemplateValue, out bool hasCorporateDocument) && hasCorporateDocument)
                    {
                        documentTypes.Add(companyDocumentType);
                    }
                }
            }
        }

        var requiredDocumentTypes = documentTypes.Where(x => x.IsRequired);

        foreach (var requiredDocumentType in requiredDocumentTypes)
        {
            if (documents is null || !documents.Any(x => x.DocumentTypeId == requiredDocumentType.DocumentTypeId))
            {
                var exceptionMessage = _exceptionsLocalizer.GetString("RequiredDocumentTypeException");
                var documentType = _documentTypesLocalizer.GetString(requiredDocumentType.Name).Value ?? requiredDocumentType.Name;

                throw new RequiredDocumentTypeException(string.Format(exceptionMessage,documentType)); 
            }
        }
    }

    private Task AddDocumentsAsync(Guid id, List<CompanyPoolDocument> documents)
    {
        if (documents.IsNullOrEmpty())
        {
            return Task.CompletedTask;
        }
        documents.ForEach(async document =>
        {
            await using var memoryStream = new MemoryStream();
            await document.File.CopyToAsync(memoryStream);

            var accountDocument = new CreateDocumentRequest()
            {
                OriginalFileName = document.File.FileName,
                ContentType = document.File.ContentType,
                Bytes = memoryStream.ToArray(),
                AccountId = id,
                DocumentTypeId = document.DocumentTypeId
            };

            await _documentService.CreateDocument(accountDocument);

        });
        return Task.CompletedTask;
    }

    private async Task ValidateCompanyPool(SaveCompanyPoolCommand request)
    {
        var company = await _repository.GetAll()
                                       .Where(x => x.TaxNumber == request.TaxNumber)
                                       .FirstOrDefaultAsync();


        if (company is null)
        {
            return;
        }

        if (company.CompanyPoolStatus == CompanyPoolStatus.Approved)
        {
            throw new DuplicateRecordException(nameof(CompanyPool));
        }

    }

    private CompanyPool PrepareCompanyPool(SaveCompanyPoolCommand request)
    {
        var createdUser = _contextProvider.CurrentContext?.UserId;
        var applicationUserId = _applicationUserService.ApplicationUserId;
        var newCompany = new CompanyPool
        {
            CompanyPoolStatus = CompanyPoolStatus.Waiting,
            CompanyType = request.CompanyType,
            Address = request.Address,
            AuthorizedPersonBirthDate = request.AuthorizedPersonBirthDate,
            AuthorizedPersonCompanyPhoneCode = request.AuthorizedPersonCompanyPhoneCode,
            AuthorizedPersonCompanyPhoneNumber = request.AuthorizedPersonCompanyPhoneNumber,
            AuthorizedPersonIdentityNumber = request.AuthorizedPersonIdentityNumber,
            AuthorizedPersonName = request.AuthorizedPersonName,
            AuthorizedPersonSurname = request.AuthorizedPersonSurname,
            City = request.City,
            CityName = request.CityName,
            Email = request.Email,
            Title = request.Title,
            Country = request.Country,
            CountryName = request.CountryName,
            District = request.District,
            DistrictName = request.DistrictName,
            Iban = request.Iban,
            LandPhone = request.LandPhone,
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            PostalCode = request.PostalCode,
            TaxAdministration = request.TaxAdministration,
            TaxNumber = request.TaxNumber,
            WebSiteUrl = request.WebSiteUrl,
            RecordStatus = RecordStatus.Active,
            CreatedBy = !string.IsNullOrWhiteSpace(createdUser) ? createdUser : applicationUserId.ToString(),
            MersisNumber = request.MersisNumber,
            Channel = request.Channel,
            AuthorizedPersonEmail = request.AuthorizedPersonEmail,
        };
        return newCompany;
    }
}