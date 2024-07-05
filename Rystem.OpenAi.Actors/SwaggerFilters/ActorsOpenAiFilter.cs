extern alias Rystem;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rystem.OpenAi.Actors
{
    public class ActorsOpenAiFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters.Count > 0 || context.ApiDescription.ParameterDescriptions.Count > 0)
            {
                var relativePath = context.ApiDescription.RelativePath;
                if (relativePath == "api/ai/message")
                    return;
                var name = relativePath.Replace("/", "_");
                var jsonFunctionObject = new JsonFunctionNonPrimitiveProperty()
                {
                    Type = "object",
                    Description = operation.Description ?? relativePath,
                };
                var jsonFunction = new JsonFunction
                {
                    Name = name,
                    Description = operation.Description ?? name,
                    Parameters = jsonFunctionObject
                };
                foreach (var scene in ScenesBuilderHelper.FunctionsForEachScene)
                {
                    foreach (var path in scene.Value.AvailableApiPath)
                    {
                        if (path.IsMatch(relativePath))
                        {
                            scene.Value.Functions.Add(jsonFunction);
                            break;
                        }
                    }
                }
                ScenesBuilderHelper.Actions.Add(name, new());
                ScenesBuilderHelper.Calls.Add(name, (httpBringer) =>
                {
                    httpBringer.Method = context.ApiDescription.HttpMethod;
                    return ValueTask.CompletedTask;
                });
                foreach (var parameter in context.ApiDescription.ParameterDescriptions)
                {
                    var parameterName = parameter.Name ?? parameter.Type.Name;
                    Add(parameterName, parameter.Type, jsonFunctionObject);
                    if (parameter.Source == BindingSource.Query)
                    {
                        ScenesBuilderHelper.Actions[name][parameterName] = async (value, httpBringer) =>
                        {
                            if (httpBringer.Query is null)
                                httpBringer.Query = new();
                            httpBringer.Query.Append($"{parameterName}={value[parameterName]}&");
                        };
                    }
                    else if (parameter.Source == BindingSource.Body)
                    {
                        ScenesBuilderHelper.Actions[name][parameterName] = (value, httpBringer) =>
                        {
                            httpBringer.BodyAsJson = value[parameterName];
                            return ValueTask.CompletedTask;
                        };
                    }


                }
                void Add(string? parameterName, Type type, JsonFunctionNonPrimitiveProperty jsonFunction)
                {
                    var description = type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (Rystem::System.Reflection.PrimitiveExtensions.IsPrimitive(type))
                    {
                        jsonFunction.AddPrimitive(parameterName ?? type.Name, new JsonFunctionProperty
                        {
                            Description = description?.Description ?? parameterName ?? type.Name,
                            Type = Rystem::System.Reflection.PrimitiveExtensions.IsNumeric(type) ? "number" : "string"
                        });
                    }
                    else
                    {
                        var innerFunction = new JsonFunctionNonPrimitiveProperty()
                        {
                            Description = description?.Description ?? parameterName ?? type.Name,
                        };
                        jsonFunction.AddObject(parameterName ?? type.Name, innerFunction);
                        foreach (var innerParameter in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (innerParameter.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() is null)
                            {
                                Add(innerParameter.Name, innerParameter.PropertyType, innerFunction);
                            }
                        }
                    }
                }
            }
        }
    }
}
