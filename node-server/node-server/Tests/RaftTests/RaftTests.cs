using NodeServer.Managers;
using NodeServer.Managers.RaftNameSpace;
using NodeServer.Services;
using System.Net;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;


namespace Tests.RaftTests
{
    public class GlobalVariables
    {
        public static string args = "";
        public static string path = "";
    }
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            RaftSettings settings = new RaftSettings();
            settings.ServersPort = int.Parse(GlobalVariables.args);
            settings.ServerId = int.Parse(GlobalVariables.args);
            settings.LogFilePath = GlobalVariables.path;
            settings.ServerAddress = $"127.0.0.1:{settings.ServersPort}";
            settings.ServersAddresses = new List<string> { "127.0.0.1:1111", "127.0.0.1:2222", "127.0.0.1:3333" };

            FileSaving micro = new FileSaving("127.0.0.1", 50051);
            FileVersionManager db = new FileVersionManager("FileManager.db");

            services.AddSingleton<FileSaving>(micro);
            services.AddSingleton<FileVersionManager>(db);
            services.AddSingleton<Raft>(new Raft(settings, micro, db));

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
            Console.WriteLine(args[0]);
            GlobalVariables.args = args[0];
            GlobalVariables.path = args[1];
            CreateHostBuilder(new string[0], args[0]).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string address) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://0.0.0.0:{address}");

                });
    }
}
