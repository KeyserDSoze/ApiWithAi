using System.Text.Json.Serialization;

namespace Rystem.OpenAi.Actors
{
    internal static class ScenesBuilderHelper
    {
        public static List<JsonFunction> ScenesAsFunctions { get; } = new List<JsonFunction>();
        public static Dictionary<string, ScenesJsonFunctionWrapper> FunctionsForEachScene { get; } = new();
        public static Dictionary<string, Dictionary<string, Func<Dictionary<string, string>, HttpBringer, ValueTask>>> Actions { get; } = new();
        public static Dictionary<string, Func<HttpBringer, ValueTask>> Calls { get; } = new();
    }
}
