using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Authorization.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public class PermissionHttpClient : HttpClientBase, IPermissionHttpClient
{
    public PermissionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        var response = await GetAsync($"v1/Permissions");

        var responseString = await response.Content.ReadAsStringAsync();

        var permissions = JsonSerializer.Deserialize<List<PermissionDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return permissions ?? throw new InvalidOperationException();
    }

    public async Task UpdateAsync(Guid permissionId, UpdatePermissionRequest request)
    {
        await PutAsJsonAsync($"v1/Permissions/{permissionId}", request);
    }

    public async Task SyncPermissionsAsync()
    {
        const string controllerPrefix = "Controller";
        List<SyncPermissionRequest> requestList = new();

        IsAssignableFrom(controllerPrefix, requestList);

        if (requestList.Count > 0)
        {
            await PutAsJsonAsync("v1/Permissions/synchronise", requestList);
        }
    }

    private void IsAssignableFrom(string controllerPrefix, List<SyncPermissionRequest> requestList)
    {
        foreach (var controllerType in Assembly
          .GetExecutingAssembly()
          .GetExportedTypes()
          .Where(q => typeof(ControllerBase).IsAssignableFrom(q)))
        {
            BindingFlags(controllerPrefix, requestList, controllerType);
        }
    }

    private void BindingFlags(string controllerPrefix, List<SyncPermissionRequest> requestList, Type controllerType)
    {
        foreach (var actionMethod in controllerType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
        {
            var controllerName = controllerType.Name.Substring(0, controllerType.Name.Length - controllerPrefix.Length);

            var actionName = actionMethod.Name;

            var attributesList = actionMethod.CustomAttributes
                .Where(q => q.AttributeType == typeof(AuthorizeAttribute))
                .ToList();

            AttributesList(requestList, controllerName, actionName, attributesList);
        }
    }

    private void AttributesList(List<SyncPermissionRequest> requestList, string controllerName, string actionName, List<CustomAttributeData> attributesList)
    {
        foreach (var attributes in attributesList)
        {
            var attribute = attributes.NamedArguments.Where(q => q.MemberName == "Policy").ToList();

            foreach (var myAttribute in attribute)
            {
                if (myAttribute.MemberName == "Policy")
                {
                    var splitedClaims = myAttribute.TypedValue.Value.ToString().Split(':');
                    var operationType = GetEnumValue(splitedClaims.LastOrDefault());
                    var moduleName = splitedClaims.FirstOrDefault();

                    requestList.Add(new SyncPermissionRequest()
                    {
                        ClaimType = "BackOffice",
                        ClaimValue = myAttribute.TypedValue.Value.ToString(),
                        Module = moduleName,
                        DisplayName = moduleName,
                        Description = $"{controllerName} {actionName} - {splitedClaims.LastOrDefault()}",
                        Display = true,
                        OperationType = operationType
                    });
                }
            }
        }
    }

    private static PermissionOperationType GetEnumValue(string operationType)
       => operationType switch
       {
           nameof(PermissionOperationType.Create) => PermissionOperationType.Create,
           nameof(PermissionOperationType.Update) => PermissionOperationType.Update,
           nameof(PermissionOperationType.Delete) => PermissionOperationType.Delete,
           nameof(PermissionOperationType.ReadAll) => PermissionOperationType.ReadAll,
           _ => PermissionOperationType.Read
       };

}