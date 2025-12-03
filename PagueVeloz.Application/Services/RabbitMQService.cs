using Microsoft.Extensions.Configuration;
using PagueVeloz.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WebApiBiblioteca.Service.RabbitMQ;

namespace WebApiBiblioteca.Infra
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly IConfiguration _configuration;

        public RabbitMQService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void PublicarMensagem(object mensagem, string fila)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: fila, durable: false, exclusive: false, autoDelete: false, arguments: null);

            string json = JsonSerializer.Serialize(mensagem);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: fila, basicProperties: null, body: body);
        }
    }


}
