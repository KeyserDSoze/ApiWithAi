using System.Reflection;
using System.Text.RegularExpressions;
using ApiWithAi.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddOpenAi(x =>
{
    x.ApiKey = builder.Configuration["OpenAi:Key"];
    x.Azure.ResourceName = builder.Configuration["OpenAi:Name"];
    x.Version = "2024-02-01";
    x.Azure.MapDeploymentChatModel("gpt4ogs", Rystem.OpenAi.ChatModelType.Gpt4_32K_Snapshot);
});
builder.Services.AddHttpClient("apiDomain", x =>
{
    x.BaseAddress = new Uri("https://localhost:7008/");
});
builder.Services.AddPlayFramework(scenes =>
{
    scenes.Configure(settings =>
    {
        settings.OpenAi.Name = null;
    })
    .AddScene(scene =>
    {
        scene
            .WithName("Weather")
            .WithDescription("Get information about the weather")
            .WithHttpClient("apiDomain")
            .WithOpenAi(null)
            .WithApi(pathBuilder =>
            {
                pathBuilder
                    .Map(new Regex("Country/*"))
                    .Map(new Regex("City/*"))
                    .Map("Weather/");
            })
                .WithActors(actors =>
                {
                    actors
                        .AddActor("Nel caso non esistesse la città richiesta potresti aggiungerla con il numero dei suoi abitanti.")
                        .AddActor("Ricordati che va sempre aggiunta anche la nazione, quindi se non c'è la nazione aggiungi anche quella.")
                        .AddActor("Non chiamare alcun meteo prima di assicurarti che tutto sia stato popolato correttamente.")
                        .AddActor<ActorWithDbRequest>();
                });
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.UseAiEndpoints();

app.Run();
