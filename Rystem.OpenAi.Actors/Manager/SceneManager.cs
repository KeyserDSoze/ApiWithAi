using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Rystem.OpenAi.Chat;

namespace Rystem.OpenAi.Actors
{
    internal sealed class SceneManager : ISceneManager
    {
        private readonly HttpContext? _httpContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOpenAiFactory _openAiFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SceneManagerSettings? _settings;

        public SceneManager(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, IOpenAiFactory openAiFactory, IHttpClientFactory httpClientFactory, SceneManagerSettings? settings = null)
        {
            _httpContext = httpContextAccessor?.HttpContext;
            _serviceProvider = serviceProvider;
            _openAiFactory = openAiFactory;
            _httpClientFactory = httpClientFactory;
            _settings = settings;
        }
        public async IAsyncEnumerable<AiSceneResponse> ExecuteAsync(string message, CancellationToken cancellationToken)
        {
            var openai = _openAiFactory.Create(_settings?.OpenAi.Name);
            var request = openai.Chat.RequestWithSystemMessage($"Oggi è {DateTime.UtcNow}.");
            foreach (var function in ScenesBuilderHelper.ScenesAsFunctions)
            {
                request.WithFunction(function);
            }
            request.AddUserMessage(message)
                .WithModel(ChatModelType.Gpt4_32K_Snapshot);
            var response = await request.ExecuteAsync(false, cancellationToken);
            if (response.Choices?[0].Message?.ToolCalls?.Count > 0)
            {
                foreach (var toolCall in response.Choices[0].Message!.ToolCalls!)
                {
                    var functionName = toolCall.Function!.Name;
                    yield return new AiSceneResponse
                    {
                        Name = functionName,
                        Message = "Starting"
                    };
                    var scene = _serviceProvider.GetKeyedService<IScene>(functionName);
                    if (scene != null)
                    {
                        await foreach (var sceneResponse in GetResponseFromSceneAsync(scene, message))
                            yield return sceneResponse;
                    }
                }
            }
            else
            {
                yield return new AiSceneResponse
                {
                    Message = response.Choices[0].Message.Content.ToString()
                };
            }
        }
        private async IAsyncEnumerable<AiSceneResponse> GetResponseFromSceneAsync(IScene scene, string message)
        {
            var openAi = _openAiFactory.Create(scene.OpenAiFactoryName);
            var request = openAi.Chat.RequestWithSystemMessage($"Oggi è {DateTime.UtcNow}.");
            var actorBuilder = new StringBuilder();
            if (scene is Scene internalScene)
                actorBuilder.Append(internalScene.SimpleActors);
            foreach (var actor in _serviceProvider.GetKeyedServices<IActor>(scene.Name))
            {
                actorBuilder.Append($"{await actor.GetMessageAsync()}.\n");
            }
            foreach (var function in ScenesBuilderHelper.FunctionsForEachScene[scene.Name].Functions)
            {
                request.WithFunction(function);
            }
            request.AddUserMessage($"{actorBuilder}\n{message}")
                .WithModel(ChatModelType.Gpt4_32K_Snapshot);
            var response = await request.ExecuteAsync(false, default);
            await foreach (var result in GetResponseAsync(scene.Name, scene.HttpClientName, request, response))
            {
                yield return result;
            }
        }
        private async IAsyncEnumerable<AiSceneResponse> GetResponseAsync(string sceneName, string? clientName, ChatRequestBuilder chatRequestBuilder, ChatResult response)
        {
            if (response.Choices?.Count > 0 && response.Choices[0].Message?.ToolCalls?.Count > 0)
            {
                foreach (var toolCall in response.Choices[0].Message!.ToolCalls!)
                {
                    var json = toolCall.Function!.Arguments!;
                    var functionName = toolCall.Function.Name!;
                    var responseAsJson = await ExecuteHttpClientAsync(clientName, functionName, json);
                    yield return new AiSceneResponse
                    {
                        Name = sceneName,
                        FunctionName = functionName,
                        Arguments = json,
                        Response = responseAsJson,
                    };
                    chatRequestBuilder.AddSystemMessage($"Response for function {functionName}: {responseAsJson}");
                }
            }
            response = await chatRequestBuilder.ExecuteAsync(false, default);
            if (response.Choices?.Count > 0 && response.Choices[0].Message?.ToolCalls?.Count > 0)
            {
                await foreach (var result in GetResponseAsync(sceneName, clientName, chatRequestBuilder, response))
                {
                    yield return result;
                }
            }
            else
                yield return new AiSceneResponse
                {
                    Name = sceneName,
                    Message = response.Choices![0].Message!.Content!.ToString()!
                };
        }
        private async Task<string?> ExecuteHttpClientAsync(string? clientName, string functionName, string argumentAsJson)
        {
            var uri = functionName.Replace("_", "/");
            var json = ParseJson(argumentAsJson);
            var httpBringer = new HttpBringer();
            await ScenesBuilderHelper.Calls[functionName](httpBringer);
            var currentActions = ScenesBuilderHelper.Actions[functionName];
            foreach (var actions in currentActions)
            {
                await actions.Value(json, httpBringer);
            }
            using var httpClient = clientName == null ? _httpClientFactory.CreateClient() : _httpClientFactory.CreateClient(clientName);
            var message = new HttpRequestMessage
            {
                Content = httpBringer.BodyAsJson != null ? new StringContent(httpBringer.BodyAsJson, Encoding.UTF8, "application/json") : null,
                Headers = { { "Accept", "application/json" } },
                RequestUri = new Uri($"{httpClient.BaseAddress}{uri}{(httpBringer.Query != null ? (uri.Contains('?') ? $"&{httpBringer.Query}" : $"?{httpBringer.Query}") : string.Empty)}"),
                Method = new HttpMethod(httpBringer.Method)
            };
            var authorization = _httpContext?.Request?.Headers?.Authorization.ToString();
            if (authorization != null)
            {
                var bearer = authorization.Split(' ');
                if (bearer.Length > 1)
                    message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(bearer[0], bearer[1]);
            }
            var request = await httpClient.SendAsync(message);
            var responseString = await request.Content.ReadAsStringAsync();
            return responseString;
        }
        private static Dictionary<string, string> ParseJson(string json)
        {
            var result = new Dictionary<string, string>();

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                foreach (var element in document.RootElement.EnumerateObject())
                {
                    if (element.Value.ValueKind == JsonValueKind.Object || element.Value.ValueKind == JsonValueKind.Array)
                    {
                        // Use GetRawText() to keep the JSON structure as a string
                        result.Add(element.Name, element.Value.GetRawText());
                    }
                    else
                    {
                        // Convert simple values directly to string
                        result.Add(element.Name, element.Value.ToString());
                    }
                }
            }
            return result;
        }
    }
}
