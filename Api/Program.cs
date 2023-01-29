using Api.Configuration;
using Api.Helpers;
using Api.Helpers.Interfaces;
using Api.Services;
using Api.Services.Interfaces;
using Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/sign-in");
        options.LogoutPath = new PathString("/sign-out");
    });

builder.Services.AddSingleton<ISyndicationConverter, SyndicationConverter>();

builder.Services.Configure<AppConfiguration>(builder.Configuration);

builder.Services.AddDomainServices((sp, options) =>
{
    IOptionsMonitor<AppConfiguration> configuration = sp.GetRequiredService<IOptionsMonitor<AppConfiguration>>();
    _ = options.UseSqlite(configuration.CurrentValue.ConnectionString);
});


builder.Services.AddSingleton<IAggregator, Aggregator>();
builder.Services.AddHostedService<BackgroundAggregationService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
