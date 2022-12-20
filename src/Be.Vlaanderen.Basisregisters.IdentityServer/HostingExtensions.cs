namespace IdentityServer;

using Config;
using Newtonsoft.Json;
using Serilog;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureIdentityServer(this WebApplicationBuilder builder)
    {
        var finalJsonConfig = GetJsonConfig(builder);

        builder.Services.AddIdentityServer(
                options =>
                {
                    // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                    options.EmitStaticAudienceClaim = true;
                })
            .AddInMemoryIdentityResources(finalJsonConfig.GetIdentityResources())
            .AddInMemoryApiScopes(finalJsonConfig.GetApiScopes())
            .AddInMemoryApiResources(finalJsonConfig.GetApiResources())
            .AddInMemoryClients(finalJsonConfig.GetClients());

        return builder;
    }

    private static JsonConfig GetJsonConfig(WebApplicationBuilder builder)
    {
        var finalJsonConfig = new JsonConfig();

        var configFolder = GetConfigFolder(builder);
        var fileInfos = Directory.GetFiles(configFolder, "*.json");

        if (!fileInfos.Any())
        {
            Console.WriteLine($"No config files found at {configFolder}");
            return finalJsonConfig;
        }

        foreach (var fi in fileInfos)
        {
            Console.WriteLine($"Processing {fi}");
            try
            {
                var json = File.ReadAllText(fi);
                var jsonConfig = JsonConvert.DeserializeObject<JsonConfig>(json);

                foreach (var client in jsonConfig.GetClients())
                {
                    Console.WriteLine($"client {client.ClientId} parsed successfully");
                }

                finalJsonConfig = JsonConfig.Merge(finalJsonConfig, jsonConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return finalJsonConfig;
    }

    private static string GetConfigFolder(WebApplicationBuilder builder)
    {
        var identityServerConfig = builder.Configuration.GetSection("IdentityServer");
        var configFolder = identityServerConfig["ConfigFolder"];

        return configFolder;
    }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddCors(options => options.AddDefaultPolicy(policyBuilder => policyBuilder.AllowAnyOrigin()));
        builder.Services.ConfigureNonBreakingSameSiteCookies();

        builder.ConfigureIdentityServer();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        app.UseCookiePolicy();

        // This will write cookies, so make sure it's after the cookie policy
        app.UseAuthentication();


        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
