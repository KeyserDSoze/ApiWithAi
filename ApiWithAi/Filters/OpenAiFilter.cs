extern alias Rystem;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiWithAi.Filters
{
    public class HttpBringer
    {
        public string Method { get; set; }
        public StringBuilder Query { get; internal set; }
        public string BodyAsJson { get; internal set; }
    }
    public class OpenAiFilter : IOperationFilter
    {
        public static readonly List<JsonFunction> JsonFunctions = new List<JsonFunction>();
        public static readonly Dictionary<string, Dictionary<string, Func<Dictionary<string, string>, HttpBringer, ValueTask>>> Actions = new();
        public static readonly Dictionary<string, Func<HttpBringer, ValueTask>> Calls = new();

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters.Count > 0 || context.ApiDescription.ParameterDescriptions.Count > 0)
            {
                var relativePath = context.ApiDescription.RelativePath;
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
                JsonFunctions.Add(jsonFunction);
                Actions.Add(name, new());
                Calls.Add(name, (httpBringer) =>
                {
                    httpBringer.Method = context.ApiDescription.HttpMethod;
                    return ValueTask.CompletedTask;
                });
                //foreach (var parameter in operation.Parameters)
                //{
                //    jsonFunctionObject.AddPrimitive(parameter.Name, new JsonFunctionProperty
                //    {
                //        Description = parameter.Description,
                //        Type = parameter.Schema.Type
                //    });
                //    Actions[name][parameter.Name] = (value, httpBringer) =>
                //    {
                //        httpBringer.Query.Append($"{parameter.Name}={value[parameter.Name]}&");
                //        return ValueTask.CompletedTask;
                //    };
                //}
                foreach (var parameter in context.ApiDescription.ParameterDescriptions)
                {
                    var parameterName = parameter.Name ?? parameter.Type.Name;
                    Add(parameterName, parameter.Type, jsonFunctionObject);
                    if (parameter.Source == BindingSource.Query)
                    {
                        Actions[name][parameterName] = async (value, httpBringer) =>
                        {
                            if (httpBringer.Query is null)
                                httpBringer.Query = new();
                            httpBringer.Query.Append($"{parameterName}={value[parameterName]}&");
                        };
                    }
                    else if (parameter.Source == BindingSource.Body)
                    {
                        Actions[name][parameterName] = (value, httpBringer) =>
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
