using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Einstein.Rabbit
{
    // Kaynak: https://github.com/PacktPublishing/Adopting-.NET-5--Architecture-Migration-Best-Practices-and-New-Features/tree/master/Chapter04/microservicesapp
    public interface IMQClient
    {
        IModel CreateChannel();
    }

    public interface IMessageQueueSender
    {
        public void Send(string queueName, string message);
    }

    public static class Constants
    {
        public const string RABBIT_HOST = "SERVICE__RABBITMQ__MQ_BINDING__HOST";
        public const string RABBIT_PORT = "SERVICE__RABBITMQ__MQ_BINDING__PORT";
        public const string RABBIT_ALT_HOST = "SERVICE__RABBITMQ__HOST";
        public const string RABBIT_ALT_PORT = "SERVICE__RABBITMQ__PORT";
        public const string RABBIT_ALT2_PORT = "RABBITMQ_SERVICE_PORT";
        public const string RABBIT_USER = "RABBIT_USER";
        public const string RABBIT_PSWD = "RABBIT_PSWD";
        public const string RABBIT_QUEUE = "RABBIT_QUEUE";

        public static string GetRabbitMQHostName()
        {
            var v = Environment.GetEnvironmentVariable(RABBIT_HOST);
            if (string.IsNullOrWhiteSpace(v))
            {
                v = Environment.GetEnvironmentVariable(RABBIT_ALT_HOST);
                if (string.IsNullOrWhiteSpace(v))
                    return "rabbitmq";
                else return v;
            }
            else return v;
        }

        public static string GetRabbitMQPort()
        {
            var v = Environment.GetEnvironmentVariable(RABBIT_PORT);
            if (string.IsNullOrWhiteSpace(v))
            {
                v = Environment.GetEnvironmentVariable(RABBIT_ALT_PORT);
                if (string.IsNullOrWhiteSpace(v) || v == "-1")
                    return Environment.GetEnvironmentVariable(RABBIT_ALT2_PORT);
                else return v;
            }
            else return v;
        }

        public static string GetRabbitMQUser()
        {
            var v = Environment.GetEnvironmentVariable(RABBIT_USER);
            if (string.IsNullOrWhiteSpace(v))
                return "guest"; 
            else return v;
        }

        public static string GetRabbitMQPassword()
        {
            var v = Environment.GetEnvironmentVariable(RABBIT_PSWD);
            if (string.IsNullOrWhiteSpace(v))
                return "guest";
            else return v;
        }

        public static string GetRabbitMQQueueName()
        {
            var v = Environment.GetEnvironmentVariable(RABBIT_QUEUE);
            if (string.IsNullOrWhiteSpace(v))
                return "palindromes"; // Consumer'ın dinyeceği varsayılan kuyruk adı. Normalde RABBIT_QUEUE ile çevre değişken üzerinden gelmezse bu kullanılır.
            else return v;
        }
    }

    public class RabbitMQClient : IMQClient
    {
        public string hostname { get; }
        public string port { get; }
        public string userid { get; }
        public string password { get; }

        private readonly ILogger _logger;
        private readonly IConnection _connection;
        private IModel _channel;

        public RabbitMQClient(ILogger<RabbitMQClient> logger, IConfiguration configuration)
        {
            _logger = logger;

            hostname = Constants.GetRabbitMQHostName();
            port = Constants.GetRabbitMQPort();
            userid = Constants.GetRabbitMQUser();
            password = Constants.GetRabbitMQPassword();

            try
            {
                logger.LogInformation($"RabbitMQ Bağlantısı oluşturuluyor. @ {hostname}:{port}:{userid}:{password}");
                var factory = new ConnectionFactory()
                {
                    HostName = hostname,
                    Port = int.Parse(port),
                    UserName = userid,
                    Password = password,
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                logger.LogError(-1, ex, "RabbitMQ Bağlantısı oluşturulması sırasında hata oluştu.");
                throw;
            }
        }

        public IModel CreateChannel()
        {
            if (_connection == null)
            {
                _logger.LogError("RabbiMQ Kanal bağlantısı oluşturulması sırasında hata oluştu.");
                throw new Exception("RabbitMQClient bağlantı hatası.");
            }
            _channel = _connection.CreateModel();
            return _channel;
        }
    }

    public class RabbitMQueueSender : IMessageQueueSender
    {
        private readonly ILogger<RabbitMQueueSender> _logger;
        private readonly IMQClient _mqClient;

        private IModel _mqChannel;
        private string _queueName;

        private IModel MQChannel
        {
            get
            {
                if (_mqChannel == null || _mqChannel.IsClosed)
                    _mqChannel = _mqClient.CreateChannel();
                return _mqChannel;
            }
        }

        public RabbitMQueueSender(ILogger<RabbitMQueueSender> logger, IMQClient mqClient)
        {
            _logger = logger;
            _mqClient = mqClient;
        }

        public void Send(string queueName, string message)
        {
            if (string.IsNullOrWhiteSpace(queueName)) return; 

            if (string.IsNullOrWhiteSpace(_queueName)) 
            {
                _logger.LogInformation($"{queueName} isimli kuyruk ilk kez oluşturuluyor.");
                MQChannel.QueueDeclare(queue: queueName,
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
                _queueName = queueName;
            }

            _logger.LogInformation($"Mesaj kuyruğunu gönderiliyor. Queue Name:{queueName}");

            var body = Encoding.UTF8.GetBytes(message);

            try
            {
                MQChannel.BasicPublish(exchange: "",
                                            routingKey: queueName,
                                            basicProperties: null,
                                            body: body);
            }
            catch (System.Exception ex)
            {
                ex.ToString();
            }
            _logger.LogInformation("Mesaj başarılı bir şekilde kuyruğa aktarıldı.");
        }
    }

    public static class RabbitMQServiceCollectionExtensions
    {
        // Startup.cs'de RabbitMQ'yu servis listesine eklememizi sağlayan genişletme fonksiyonu
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Add(ServiceDescriptor.Singleton<IMQClient, RabbitMQClient>());
            services.Add(ServiceDescriptor.Singleton<IMessageQueueSender, RabbitMQueueSender>());

            return services;
        }
    }
}
