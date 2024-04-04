using NodeServer.Services;
using NodeServer.Managers;
using NodeServer.Managers.RaftNameSpace;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        RaftSettings raftSettings = new RaftSettings();
        FileSaving micro = new FileSaving("127.0.0.1", 50051);
        FileVersionManager db = new FileVersionManager("FileManager.db");
        Console.WriteLine($"ips:");
        foreach (var ip in raftSettings.ServersAddresses) 
        {
            Console.WriteLine(ip);
        }
        services.AddSingleton<FileSaving>(micro);
        services.AddSingleton<FileVersionManager>(db);
        services.AddSingleton<Raft>(new Raft(raftSettings, micro, db));

        services.AddGrpc();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ServerToServerService>();
            endpoints.MapGrpcService<NodeServerService>();
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
                //webBuilder.UseUrls("http://localhost:50052");
                webBuilder.UseUrls("http://0.0.0.0:50052");

                /*
                webBuilder.ConfigureKestrel(options =>
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();

                    var ipAddress = config.GetValue<string>("Server:IPAddress");
                    var port = config.GetValue<int>("Server:Port");

                    options.Listen(IPAddress.Parse(ipAddress), port);
                });
                */
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders(); 
                logging.AddConsole();
            });
}
