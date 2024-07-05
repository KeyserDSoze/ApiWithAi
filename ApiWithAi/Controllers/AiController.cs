using Microsoft.AspNetCore.Mvc;
using Rystem.OpenAi;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiWithAi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AiController : ControllerBase
    {
        public AiController(ISwaggerProvider swaggerProvider, IOpenAiFactory openAi)
        {
            _swaggerProvider = swaggerProvider;
            OpenAi = openAi.Create();
        }

        private readonly ISwaggerProvider _swaggerProvider;

        public IOpenAi OpenAi { get; }

        /// <summary>
        /// Get the swagger documentation.
        /// </summary>
        /// <remarks>
        /// This endpoint expects a swagger.
        /// </remarks>
        /// <returns>All documentation in json format.</returns>
        /// <example>
        /// GET /SwaggerDocs
        /// </example>
        [HttpGet]
        public async Task<string> GetAsync([FromQuery] string? request = null)
        {
            //            var kernel = new Kernel();
            //            using HttpClient httpClient = new();

            //#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            //            KernelPlugin plugin = await kernel.ImportPluginFromOpenApiAsync(
            //                "Weather",
            //                new Uri("https://localhost:7008/swagger/v1/swagger.json"),
            //                new OpenApiFunctionExecutionParameters(httpClient),
            //                default);
            //            var qul = await kernel.InvokeAsync(plugin.First(), new KernelArguments
            //            {
            //                ["city"] = "New York"
            //            });
            //#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            //var q = OpenAi.Chat.RequestWithSystemMessage($"Oggi è {DateTime.UtcNow}");
            //foreach (var function in OpenAiFilter.JsonFunctions)
            //{
            //    q.WithFunction(function);
            //}
            //q.AddUserMessage(request ?? "Nel caso non esistesse New York come citta potresti aggiungerla con il numero dei suoi abitanti e poi chiedere che tempo fa oggi? Ricordati che va sempre aggiunta anche la nazione, quindi se non c'è la nazione aggiungi anche quella.")
            //    .WithModel(ChatModelType.Gpt4_32K_Snapshot);
            //var response = await q.ExecuteAsync(false, default);
            //response = await GetResponseAsync(q, response);
            //return response.Choices![0]!.Message!.Content!.ToString()!;
            return null!;
        }
    }
}
