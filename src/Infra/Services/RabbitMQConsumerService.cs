using System.Text;
using System.Text.Json;
using Application.Controllers.Estoque;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infra.Services;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string Exchange = "tech.saga";

    public RabbitMQConsumerService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        await BindQueue("estoque.os.aberta", "os.aberta", stoppingToken);
        await BindQueue("estoque.os.cancelada", "os.cancelada", stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync("estoque.os.aberta", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("estoque.os.cancelada", autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task BindQueue(string queue, string routingKey, CancellationToken ct)
    {
        await _channel!.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await _channel.QueueBindAsync(queue, Exchange, routingKey, cancellationToken: ct);

        var dlq = $"{queue}.dlq";
        await _channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        var routingKey = ea.RoutingKey;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dataSource = scope.ServiceProvider.GetRequiredService<IItemEstoqueDataSource>();
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var systemUserId = Guid.Parse(_configuration["SystemUserId"] ?? Guid.Empty.ToString());
            var controller = new EstoqueController(dataSource);

            switch (routingKey)
            {
                case "os.aberta":
                {
                    // OS aberta: oportunidade futura de pré-reservar itens quando payload incluir itens
                    var payload = JsonSerializer.Deserialize<OsAberta>(body)!;
                    foreach (var item in payload.Itens ?? [])
                    {
                        await controller.ReservarEstoque(payload.OrdemServicoId, item.ProdutoId, item.Quantidade, eventPublisher, default);
                    }
                    break;
                }
                case "os.cancelada":
                {
                    var payload = JsonSerializer.Deserialize<OsCancelada>(body)!;
                    foreach (var item in payload.Itens ?? [])
                    {
                        await controller.LiberarEstoque(payload.OrdemServicoId, item.ProdutoId, item.Quantidade, eventPublisher, default);
                    }
                    break;
                }
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    private record OsAberta(Guid OrdemServicoId, Guid ClienteId, List<OsItem>? Itens);
    private record OsCancelada(Guid OrdemServicoId, string Motivo, List<OsItem>? Itens);
    private record OsItem(Guid ProdutoId, int Quantidade);
}
