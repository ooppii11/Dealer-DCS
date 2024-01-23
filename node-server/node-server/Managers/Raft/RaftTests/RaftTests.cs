using NodeServer.Managers.RaftNameSpace;
using NodeServer.Services;
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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    Console.WriteLine("PORT:");
                    webBuilder.UseUrls($"http://0.0.0.0:{Console.ReadLine()}");


                });
    }
}
