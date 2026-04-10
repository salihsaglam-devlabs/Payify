using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LinkPara.Card.API.Helpers.Swagger;

public sealed class EnumDescriptionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is null || context?.Type is null)
            return;

        var type = context.Type;
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (!type.IsEnum || schema.Enum is null || schema.Enum.Count == 0)
            return;

        var names = Enum.GetNames(type);
        var values = Enum.GetValues(type).Cast<object>().ToArray();

        var enumNames = new OpenApiArray();
        foreach (var name in names)
            enumNames.Add(new OpenApiString(name));
        schema.Extensions["x-enumNames"] = enumNames;

        var enumValues = new OpenApiArray();
        foreach (var value in values)
            enumValues.Add(new OpenApiLong(Convert.ToInt64(value)));
        schema.Extensions["x-enumValues"] = enumValues;

        var descriptions = names.Select(name => GetEnumDescription(type, name)).ToArray();

        var arr = new OpenApiArray();
        foreach (var description in descriptions)
            arr.Add(new OpenApiString(description));

        schema.Extensions["x-enumDescriptions"] = arr;

        var sb = new StringBuilder();
        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var value = Convert.ToInt64(values[i]);
            var desc = descriptions[i];
            sb.Append("- ").Append(name).Append(" (").Append(value).Append(')');
            if (!string.IsNullOrWhiteSpace(desc) && !string.Equals(desc, name, StringComparison.Ordinal))
                sb.Append(": ").Append(desc);
            sb.AppendLine();
        }

        var mapping = sb.ToString().TrimEnd();
        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? mapping
            : $"{schema.Description}{Environment.NewLine}{Environment.NewLine}{mapping}";
    }

    private static string GetEnumDescription(Type enumType, string name)
    {
        var member = enumType.GetMember(name).FirstOrDefault();
        var attribute = member?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? name;
    }
}

