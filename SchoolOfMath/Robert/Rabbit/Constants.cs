using System;

namespace Robert.Rabbit
{
    /// Kaynak: https://github.com/PacktPublishing/Adopting-.NET-5--Architecture-Migration-Best-Practices-and-New-Features/tree/master/Chapter04/microservicesapp
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
                return "primes";
            else return v;
        }
    }
}
