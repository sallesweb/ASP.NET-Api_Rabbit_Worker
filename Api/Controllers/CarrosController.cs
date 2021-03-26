namespace Api.Controllers
{
    using System.Text;
    using System.Text.Json;
    using Core.Entities;
    using Core.Factories;
    using Microsoft.AspNetCore.Mvc;
    using RabbitMQ.Client;

    [ApiController]
    [Route("/v1/[controller]")]
    public sealed class CarrosController : ControllerBase
    {
        public IActionResult Create(
            [FromBody] Carro request,
            [FromServices] IConnectionRabbitFactory rabbit)
        {
            using (var connection = rabbit.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queue = "queue_carros";

                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var message = JsonSerializer.Serialize<Carro>(request);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: string.Empty, routingKey: queue, basicProperties: properties, body: body);

                return Accepted("Carro recebido com sucesso.");
            }
        }
    }
}