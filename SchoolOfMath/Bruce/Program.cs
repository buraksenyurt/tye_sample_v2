using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolOfRock;
using System;

namespace Bruce
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
                    services.AddGrpcClient<PalindromeFinder.PalindromeFinderClient>(options =>
                    {
                        options.Address = hostContext.Configuration.GetServiceUri("einstein") ?? new Uri("https://localhost:7001");
                    });
                    services.AddHostedService<Worker>();
                });
    }
}
