namespace Core.Factories
{
    using RabbitMQ.Client;

    public interface IConnectionRabbitFactory
    {
        IConnection CreateConnection();
    }
}