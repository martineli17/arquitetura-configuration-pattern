using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Api;

public class RabbitManager
{
    private static IConnection? _connection = null;
    public RabbitManager(string connectionString)
    {
        _connection ??= CreateConnection(connectionString);
    }

    public IModel CreateChannel() => _connection!.CreateModel();

    public async Task CreateQueueAsync(string name, IModel channel)
    {
        await Task.Run(() =>
        {
            channel.QueueDeclare(queue: name,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false);
        });
    }

    public async Task CreateExchangeAsync(string name, string type, IModel channel)
    {
        await Task.Run(() => channel.ExchangeDeclare(name, type, true, false));
    }

    public async Task CreateBindExchangeQueue(string queue, string exchange, string routerKey, IModel channel)
    {
        await Task.Run(() => channel.QueueBind(queue, exchange, routerKey));
    }

    public async Task ProducerAsync<TContent>(string exchange, string routingKey, TContent content, IModel channel)
    {
        await Task.Run(() =>
        {
            var props = channel.CreateBasicProperties();
            channel.BasicPublish(exchange, routingKey, props, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content)));
        });
    }

    public async Task ConsumerAsync(string queue, IModel channel, Func<string, Task> handle)
    {
        await Task.Run(() =>
        {
            channel.BasicQos(0, 5, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (sender, events) =>
            {
                await handle(Encoding.UTF8.GetString(events.Body.ToArray()));
                channel.BasicAck(events.DeliveryTag, false);
            };
            channel.BasicConsume(queue, false, "", false, true, null, consumer);

        });
    }

    private IConnection CreateConnection(string connectionString)
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        return factory.CreateConnection();
    }
}
