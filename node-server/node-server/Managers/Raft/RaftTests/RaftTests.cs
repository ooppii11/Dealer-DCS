using NodeServer.Managers.RaftNameSpace;
using NodeServer.Services;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;
namespace NodeServer.Managers.RaftNameSpace.RaftTestsNameSpace
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<Raft>(new Raft(new RaftSettings()));
            services.AddSingleton<Log>(new Log(""));
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ServerToServerService>();
                endpoints.MapGet("/", context => context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client."));
            });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(new string[0], args[0]).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string adress) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://127.0.0.1:{adress}");

                });
    }
}
