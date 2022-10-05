using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace TestApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        builder
            .Services
            .AddAuthentication(
                options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddOAuth2Introspection(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.ClientId = "organisation-registry-local-dev";
                    options.ClientSecret = "a_very=Secr3t*Key";
                    options.Authority = "http://localhost:5050";
                    options.IntrospectionEndpoint = "http://localhost:5050/connect/introspect";
                }
            ).Services
            .AddAuthorization(options =>
                options.AddPolicy(
                    "testclient",
                    builder => builder.RequireClaim(
                        "scope",
                        "dv_organisatieregister_testclient")
                ));

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "testclient")] () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        (
                            DateTime.Now.AddDays(index),
                            Random.Shared.Next(-20, 55),
                            summaries[Random.Shared.Next(summaries.Length)]
                        ))
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast");

        app.Run();

    }
}

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
