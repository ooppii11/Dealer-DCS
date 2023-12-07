using cloud_server.Services;
using cloud_server.Managers;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<Authentication>(new Authentication(new AuthDB("tables.sql", "172.18.0.2", "dBserver", "5432", "123AvIt456", "mydatabase")));
        services.AddGrpc();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<CloudGrpsService>();
            endpoints.MapGet("/", context => context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client."));
        });
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://172.18.0.3:5000"); // Change the port as needed
            });
}

