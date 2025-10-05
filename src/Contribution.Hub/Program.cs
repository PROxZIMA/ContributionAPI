using Contribution.Hub.Models;
using Contribution.Hub.Repository;
using Contribution.Hub.Services;
using Contribution.Hub.Managers;
using Contribution.Hub.Factory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure options
builder.Services.Configure<HubOptions>(
    builder.Configuration.GetSection(HubOptions.SectionName));

// builder.Configuration.AddEnvironmentVariables(source => source.Prefix = "HUB_");

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IUserDataRepository, UserDataRepository>();
builder.Services.AddScoped<ISecretManagerService, SecretManagerService>();
builder.Services.AddScoped<IContributionProviderFactory, ContributionProviderFactory>();
builder.Services.AddScoped<IContributionAggregatorManager, ContributionAggregatorManager>();
builder.Services.AddScoped<ISvgGeneratorService, SvgGeneratorService>();

// Register HTTP client for external service calls
builder.Services.AddHttpClient<IContributionServiceClient, ContributionServiceClient>();

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

// Add logging
builder.Services.AddLogging();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:9002", "https://c-m-app.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
});

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.Run();