using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolOfRock;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bruce
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PalindromeFinder.PalindromeFinderClient _client;

        // gRPC servisini constructor üzerinden içeriye enjekte ediyoruz
        public Worker(ILogger<Worker> logger, PalindromeFinder.PalindromeFinderClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Servisin ayağa kalkması için bir süre bekletiyoruz. Makine soğuk. 
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            _logger.LogInformation("### Servis başlatılıyor ###");
            long number = 10000; // Bruce, 10000den itibaren sayıları hesap etmeye başlayacak
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _client.IsItPalindromeAsync(new PalindromeRequest { Number = number });
                    _logger.LogInformation($"{number}, palindrom bir sayıdır önermesinin cevabı = {response.IsPalindrome}\r");
                }
                catch (Exception ex)
                {
                    // Bir exception oluşması halinde Worker'ın işleyişini durduracağız
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    _logger.LogError(-1, ex, "Bir hata oluştu. Worker çalışması sonlanıyor.");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }

                number++;

                if (stoppingToken.IsCancellationRequested)
                    break;

                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken); // İstemci 100 milisaniyede bir ateş edecek :P
            }
        }
    }
}
