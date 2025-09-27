using Contribution.Common.Auth;
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
        builder.Services.AddHttpClient();

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
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
