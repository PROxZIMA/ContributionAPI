using Contribution.Hub.Models;
using Contribution.Hub.Repository;
using Contribution.Hub.Services;
using Contribution.Hub.Managers;
using Contribution.Hub.Factory;
using Contribution.Common.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure options
builder.Services.Configure<HubOptions>(
    builder.Configuration.GetSection(HubOptions.SectionName));

// // Add authentication
// builder.Services.AddAuthentication("Custom")
//     .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, CommonAuthenticationHandler>(
//         "Custom", options => { });

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IUserDataRepository, UserDataRepository>();
builder.Services.AddScoped<ISecretManagerService, SecretManagerService>();
builder.Services.AddScoped<IContributionProviderFactory, ContributionProviderFactory>();
builder.Services.AddScoped<IContributionAggregatorManager, ContributionAggregatorManager>();

// Register HTTP client for external service calls
builder.Services.AddHttpClient<IContributionServiceClient, ContributionServiceClient>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();