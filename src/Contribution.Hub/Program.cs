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

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IUserDataRepository, UserDataRepository>();
builder.Services.AddScoped<ISecretManagerService, SecretManagerService>();
builder.Services.AddScoped<IContributionProviderFactory, ContributionProviderFactory>();
builder.Services.AddScoped<IContributionAggregatorManager, ContributionAggregatorManager>();

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

// Add response caching
builder.Services.AddResponseCaching();

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

app.UseRateLimiter();
app.UseResponseCaching();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
});

app.MapControllers();

app.Run();