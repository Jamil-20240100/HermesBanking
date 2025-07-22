using HermesAPI.Extensions;
using HermesBanking.Infrastructure.Identity;
using HermesBanking.Core.Application;
using HermesBanking.Infrastructure.Persistence;
using HermesBanking.Infrastructure.Shared;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//
// LAYERS
//

builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddApplicationLayerIoc();
builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);

//
// CONFIGURATIONS
//

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddAppiVersioningExtension();
builder.Services.AddSwaggerExtension();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();
await app.Services.RunIdentitySeedAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension(app);
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecks("/health");

app.MapControllers();

await app.RunAsync();