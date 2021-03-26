namespace Core.Factories
{
    using Microsoft.Extensions.Configuration;
    using RabbitMQ.Client;

    public sealed class ConnectionRabbitFactory : IConnectionRabbitFactory
    {
        private readonly IConfiguration _configuration;

        public ConnectionRabbitFactory(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = this._configuration.GetSection("RabbitConfiguration:HostName").Value,
                UserName = this._configuration.GetSection("RabbitConfiguration:UserName").Value,
                Password = this._configuration.GetSection("RabbitConfiguration:Password").Value
            };

            return factory.CreateConnection();
        }
    }
}