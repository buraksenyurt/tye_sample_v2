using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Robert
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        // Servis çalışmaya başladığı zaman devreye giren metodu ezip kendi istediklerimizi yaptırıyoruz.
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // RabbitMQ tarafı henüz ayağa kalkmamış olabilir diye burayı 1 dakika kadar duraksatalım
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);

                // Rabbit ile konuşmak için kullanılacak kanal nesnesi alınıyor
                var queue = CreateRabbitModel(cancellationToken);

                // queue tanımlanır
                queue.QueueDeclare(
                    queue: "palindromes",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );

                // Tanımlanan kuyruğu dinleyecek nesne örneklenir
                var consumer = new EventingBasicConsumer(queue);

                // dinlenen kuyruğa mesaj geldikçe tetiklenen olay metodu
                consumer.Received += (model, arg) =>
                {
                    var number = Encoding.UTF8.GetString(arg.Body.Span); // mesaj yakalanır
                    _logger.LogInformation($"Yeni bir palindrom sayısı bulunmuş: {number}");
                };

                queue.BasicConsume(
                    queue: "palindromes",
                    autoAck: true,
                    consumer: consumer);
            }
            catch (Exception exc)
            {
                _logger.LogError($"Bir hata oluştu {exc.Message}");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private IModel CreateRabbitModel(CancellationToken cancellationToken)
        {
            try
            {
                // Önce bağlantı oluşturmak için factory nesnesi örneklenir
                var factory = new ConnectionFactory()
                {
                    HostName = Rabbit.Constants.GetRabbitMQHostName(), // Rabbit Host adresi alınır (Environment'ten gelir)
                    Port = Convert.ToInt32(Rabbit.Constants.GetRabbitMQPort()), // Port bilgisi
                    UserName=Rabbit.Constants.GetRabbitMQUser(), // Kullanıcı adı
                    Password=Rabbit.Constants.GetRabbitMQPassword() // ve Şifre
                };

                var connection = factory.CreateConnection(); // Bağlantı nesnesi oluşturulur. Exception yoksa bağlanmış demektir.
                _logger.LogInformation("RabbitMQ ile bağlantı sağlandı");
                return connection.CreateModel(); //Queue işlemleri için kullanılacak model nesnesi döndürülür
            }
            catch (Exception exc) 
            {
                _logger.LogError($"Rabbit tarafına bağlanmaya çalışırken bir hata oluştu. {exc.Message}");
                throw;
            }
        }
    }
}
