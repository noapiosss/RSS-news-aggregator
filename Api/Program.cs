using Api.Configuration;
using Api.Helpers;
using Api.Helpers.Interfaces;
using Api.Services;
using Api.Services.Interfaces;
using Api.Validation;
using Contracts.Http;
using Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    _ = lc.ReadFrom.Configuration(builder.Configuration);
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        authenticationScheme: JwtBearerDefaults.AuthenticationScheme,
        configureOptions: options =>
        {
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new()
            {
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF32.GetBytes("Smiley face with small eyes")),
                ValidAudience = "http://localhost:5000/",
                ValidIssuer = "http://localhost:5000/",
                RequireExpirationTime = true,
                RequireAudience = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = true
            };
        }
    );
builder.Services.AddAuthorization();

builder.Services.AddSingleton<ISyndicationConverter, SyndicationConverter>();

builder.Services.Configure<AppConfiguration>(builder.Configuration);

builder.Services.AddDomainServices((sp, options) =>
{
    IOptionsMonitor<AppConfiguration> configuration = sp.GetRequiredService<IOptionsMonitor<AppConfiguration>>();
    ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    _ = options.UseSqlite(configuration.CurrentValue.ConnectionString)
        .UseLoggerFactory(loggerFactory);
});


builder.Services.AddSingleton<IAggregator, Aggregator>();
builder.Services.AddHostedService<BackgroundAggregationService>();
builder.Services.AddSingleton<ITokenHandler, Api.Services.TokenHandler>();

builder.Services.AddScoped<IValidator<SignUpRequest>, SignUpRequestValidator>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
