﻿namespace Rystem.OpenAi.Actors
{
    public interface IScene
    {
        string? OpenAiFactoryName { get; set; }
        string? HttpClientName { get; set; }
        string Name { get; set; }
        string Description { get; set; }
    }
}
