﻿namespace Rystem.OpenAi.Actors
{
    public interface ISceneBuilder
    {
        ISceneBuilder WithName(string name);
        ISceneBuilder WithDescription(string description);
        ISceneBuilder WithOpenAi(string factoryName);
        ISceneBuilder WithHttpClient(string factoryName);
        ISceneBuilder WithApi(Action<IScenePathBuilder> builder);
        ISceneBuilder WithActors(Action<IActorBuilder> builder);
    }
}
