﻿using Rystem.OpenAi.Actors;

namespace ApiWithAi.Models
{
    public sealed class City
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Population { get; set; }
    }
    public sealed class Country
    {
        public string Name { get; set; }
        public int Population { get; set; }
    }
    internal sealed class ActorWithDbRequest : IActor
    {
        public async Task<string> GetMessageAsync()
        {
            await Task.Delay(0);
            return string.Empty;
        }
    }
}
