using IdentityServer;
using Serilog;

namespace Be.Vlaanderen.Basisregisters.IdentityServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(
                "https://raw.githubusercontent.com/Informatievlaanderen/association-registry/OR-1278-add-contactgegevens/identityserver/acm.json");

            await File.WriteAllTextAsync("/home/identityserver/config/download.json", response);

            var app = builder
                .ConfigureServices()
                .ConfigurePipeline();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }
}
