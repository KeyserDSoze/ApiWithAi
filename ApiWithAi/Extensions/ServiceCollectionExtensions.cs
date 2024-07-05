using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ApiWithAi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static readonly List<JsonFunction> JsonFunctions = new List<JsonFunction>();
        public static IServiceCollection AddFunctionsFromController<T>(this IServiceCollection services)
            where T : ControllerBase
        {
            var controllerType = typeof(T);
            var methods = controllerType.GetMethods();
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var jsonFunction = new JsonFunction
                {
                    Name = method.Name,
                    Parameters = new JsonFunctionNonPrimitiveProperty
                    {
                        //Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description
                    }
                };
                //foreach (var parameter in parameters)
                //{
                //    jsonFunction.Parameters.AddPrimitive(parameter.Name, new JsonFunctionProperty
                //    {
                //        Description = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description,
                //        Type = parameter.ParameterType.Name
                //    });
                //}
                JsonFunctions.Add(jsonFunction);
            }
            return services;
        }
    }
}
