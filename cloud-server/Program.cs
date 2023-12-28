using cloud_server.Services;
using cloud_server.Managers;
using cloud_server.DB;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        /*
        services.AddSingleton<Authentication>(new Authentication(new AuthDB("DB/tables.sql", "localhost", "postgres", "5432", "123456", "postgres")));
        services.AddSingleton<FilesManager>(new FilesManager(
            new cloud_server.DB.FileMetadataDB("DB/tables.sql", "localhost", "postgres", "5432", "123456", "postgres"),
             new NodeServerCommunication[1] { new NodeServerCommunication("http://localhost:50052") }));
        */
        services.AddSingleton<Authentication>(new Authentication(new AuthDB("DB/tables.sql", "172.18.0.2", "DBserver", "5432", "123AvIt456", "mydatabase")));
        services.AddSingleton<FilesManager>(new FilesManager( new FileMetadataDB("DB/tables.sql", "172.18.0.2", "DBserver", "5432", "123AvIt456", "mydatabase")));
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
                webBuilder.UseUrls("http://0.0.0.0:50053");
                //webBuilder.UseUrls("http://localhost:50053");
            });
}

