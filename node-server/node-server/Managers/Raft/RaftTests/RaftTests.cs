using NodeServer.Managers.RaftNameSpace;
using NodeServer.Services;
using System.Net;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;
namespace NodeServer.Managers.RaftNameSpace.RaftTestsNameSpace
{
    public class Startup
    {
        private readonly string _addressId;

        public Startup(string addressId)
        {
            _addressId = addressId;
        }
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<Raft>(new Raft(new RaftSettings(_addressId)));
            services.AddGrpc();
            services.AddScoped<ServerToServerService>();

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
            Console.WriteLine(args[0]);
            CreateHostBuilder(new string[0], args[0]).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string address) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(new Startup(address));
                    webBuilder.UseUrls($"http://0.0.0.0:{address}");

                });
    }
}
