using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolOfRock;
using System;

namespace Evelyne
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // gRPC istemcisini çalışma zamanına ekliyoruz
                    services.AddGrpcClient<PalindromeFinder.PalindromeFinderClient>(options =>
                    {
                        // servis adresini Tye extension fonksiyonu üstünden çekiyoruz
                        // Eğer debug modda çalışıyorsak (tye.yaml olmadan tye run ile mesela) einstein'ın 7001 nolu adresine yönlendiriyoruz.
                        options.Address = hostContext.Configuration.GetServiceUri("einstein") ?? new Uri("https://localhost:7001");
                    });
                    services.AddHostedService<Worker>();
                });
    }
}
