using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Rystem.OpenAi.Actors
{
    internal sealed class ScenesJsonFunctionWrapper
    {
        public List<JsonFunction> Functions { get; set; } = new();
        public List<Regex> AvailableApiPath { get; set; } = new();
    }
}
