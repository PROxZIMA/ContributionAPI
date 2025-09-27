using AzureContributionsApi.Handler;
using AzureContributionsApi.Managers;
using AzureContributionsApi.Models;
using AzureContributionsApi.Strategy;
using AzureContributionsApi.Repository;
using AzureContributionsApi.Factory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace AzureContributionsApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient(); // for REST calls if needed
        builder.Services.Configure<ContributionsOptions>(builder.Configuration.GetSection("Contributions"));
        
        // Register cache service as singleton for shared caching across requests
        builder.Services.AddSingleton<IAzureDevOpsCacheManager, AzureDevOpsCacheManager>();
        
        // Register repository as scoped now that it uses singleton cache service
        builder.Services.AddScoped<IAzureDevOpsRepository, AzureDevOpsRepository>();
        
        // Register factory and service as scoped to reuse connection per request
        builder.Services.AddScoped<IAzureClientFactory, AzureClientFactory>();
        
        // Register all contribution strategies
        builder.Services.AddScoped<IContributionStrategy, CommitContributionStrategy>();
        builder.Services.AddScoped<IContributionStrategy, PullRequestContributionStrategy>();
        builder.Services.AddScoped<IContributionStrategy, WorkItemContributionStrategy>();
        
        builder.Services.AddScoped<IContributionsManager, ContributionsManager>();
        builder.Services.AddLogging();

        builder.Services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure Contributions Api", Version = "v1" });
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",

                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });

            var basicSecurityScheme = new OpenApiSecurityScheme
            {
                Name = "Basic Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Description = "Basic Authentication header using the Bearer scheme. Example: \"Authorization: Basic {base64(:PAT)}\"",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
            };

            setup.AddSecurityDefinition(basicSecurityScheme.Reference.Id, basicSecurityScheme);
            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { basicSecurityScheme, Array.Empty<string>() }
            });
        });

        builder.Services.AddAuthentication("Authentication")
            .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("Authentication", null);


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
