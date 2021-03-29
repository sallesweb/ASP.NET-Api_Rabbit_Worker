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
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Serilog;

    public class Worker : BackgroundService
    {
        private readonly IConnectionRabbitFactory _rabbit;

        public Worker(IConnectionRabbitFactory rabbit)
        {
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

                        switch (ea.RoutingKey)
                        {
                            case "queue_carros":
                                {
                                    var queueMessage = JsonSerializer
                                        .Deserialize<QueueMessage<Carro>>(message);

                                    try
                                    {
                                        Console.WriteLine($"Fabricante: {queueMessage.Entity.Fabricante}, Modelo: {queueMessage.Entity.Modelo}, Cor: {queueMessage.Entity.Cor}");
                                        break;
                                    }
                                    catch (System.Exception)
                                    {
                                        var log = $@"
                                            trace_id: {queueMessage.TraceId},
                                            entity: {JsonSerializer.Serialize<Carro>(queueMessage.Entity)},
                                            message: Erro ao persistir o carro no BD.";

                                        throw new Exception(log);
                                    }
                                }
                        }

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
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
