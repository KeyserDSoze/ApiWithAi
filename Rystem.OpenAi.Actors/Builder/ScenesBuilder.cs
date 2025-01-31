﻿using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rystem.OpenAi.Actors
{
    internal sealed class ScenesBuilder : IScenesBuilder
    {
        private readonly IServiceCollection _services;
        private readonly SceneManagerSettings _settings;
        public ScenesBuilder(IServiceCollection services)
        {
            _services = services;
            _settings = new();
        }
        public IScenesBuilder Configure(Action<SceneManagerSettings> settings)
        {
            settings(_settings);
            _services.TryAddSingleton(_settings);
            return this;
        }
        public IScenesBuilder AddScene(Action<ISceneBuilder> builder)
        {
            var sceneBuilder = new SceneBuilder(_services);
            builder(sceneBuilder);
            _services.AddKeyedSingleton(sceneBuilder.Scene.Name, sceneBuilder.Scene);
            var jsonFunction = new JsonFunction
            {
                Name = sceneBuilder.Scene.Name,
                Description = sceneBuilder.Scene.Description,
                Parameters = new JsonFunctionNonPrimitiveProperty
                {
                    Description = string.Empty,
                }
            };
            ScenesBuilderHelper.ScenesAsFunctions.Add(jsonFunction);
            ScenesBuilderHelper.FunctionsForEachScene.Add(sceneBuilder.Scene.Name, new ScenesJsonFunctionWrapper()
            {
                AvailableApiPath = sceneBuilder.RegexForApiMapping
            });
            return this;
        }
    }
}
