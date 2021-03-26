namespace Worker
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Entities;
    using Core.Factories;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConnectionRabbitFactory _rabbit;

        public Worker(
            ILogger<Worker> logger,
            IConnectionRabbitFactory rabbit)
        {
            _logger = logger;
            this._rabbit = rabbit;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var connection = this._rabbit.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queue = "queue_carros";
                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var carro = JsonSerializer.Deserialize<Carro>(message);

                        Console.WriteLine($"Fabricante: {carro.Fabricante}, Modelo: {carro.Modelo}, Cor: {carro.Cor}");

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (System.Exception)
                    {
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        throw;
                    }
                };
                channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

                Console.WriteLine("Consumindo listas...");
                Console.ReadLine();

                await Task.FromResult("Processado com sucesso.");
            }
        }
    }
}
