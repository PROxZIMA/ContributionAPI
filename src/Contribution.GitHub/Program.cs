using Contribution.Common.Auth;
using Contribution.Common.Managers;
using Contribution.Common.Models;
using Contribution.GitHub.Managers;
using Contribution.GitHub.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Contribution.GitHub;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient();
        builder.Services.Configure<ContributionsOptions>(builder.Configuration.GetSection("Contributions"));

        // Register cache service as singleton for shared caching across requests
        builder.Services.AddSingleton<ICacheManager, CacheManager>();

        builder.Services.AddScoped<IGitHubRepository, GitHubRepository>();
        builder.Services.AddScoped<IGitHubContributionsManager, GitHubContributionsManager>();

        builder.Services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1", new OpenApiInfo { Title = "GitHub Contributions Api", Version = "v1" });
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
            };
            setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            setup.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });

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
            setup.AddSecurityRequirement(new OpenApiSecurityRequirement { { basicSecurityScheme, Array.Empty<string>() } });
        });

        builder.Services.AddAuthentication("Authentication")
            .AddScheme<AuthenticationSchemeOptions, CommonAuthenticationHandler>("Authentication", null);

        // Add rate limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429; // Too Many Requests
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
            };
        });

        // Add health checks
        builder.Services.AddHealthChecks();

        var app = builder.Build();

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
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Map health check endpoints
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");
        app.Run();
    }
}
