using System.Runtime.Serialization;
using AutoMapper;
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace LinkPara.Identity.UnitTest.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void Mapper_configuration_should_valid()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Test]
    [TestCase(typeof(User), typeof(UserDto))]
    public void Mapper_should_map_from_source_to_destination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);

        _mapper.Map(instance, source, destination);
    }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return FormatterServices.GetUninitializedObject(type);
    }
}