using System.Reflection;
using System.Security.Claims;
using AutoMapper;

namespace LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

public interface IHasUserId
{
    Guid UserId { get; set; }
}

public interface IServiceRequestConverter
{
    TResponse Convert<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class, IHasUserId;
}

public class ServiceRequestConverter : IServiceRequestConverter
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public ServiceRequestConverter(IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public TResponse Convert<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class, IHasUserId
    {
        var response = _mapper.Map<TResponse>(request);

        if (response is not null)
        {
            var loggedUser = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (Guid.TryParse(loggedUser, out var userId))
            {
                response.UserId = userId;
            }
        }

        return response;
    }
}

public class GenericProfile : Profile
{
    public GenericProfile()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var types = assembly.GetTypes()
            .Where(p => typeof(IHasUserId).IsAssignableFrom(p) && p.IsClass);

        foreach (var type in types)
        {
            var source = type.BaseType;
            var destination = type;
            CreateMap(source, destination);
        }
    }
}