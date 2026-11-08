using Application.Gateways.Estoque;
using Application.Interfaces;

namespace Application.UseCases.Estoque;

public class ReservarEstoqueUseCase
{
    private readonly ItemEstoqueGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private ReservarEstoqueUseCase(ItemEstoqueGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static ReservarEstoqueUseCase Create(ItemEstoqueGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(Guid pedidoId, Guid produtoId, int quantidade, CancellationToken ct)
    {
        var item = await _gateway.GetByProdutoId(produtoId, ct);

        if (item is null || !item.Reservar(quantidade))
        {
            await _eventPublisher.PublishAsync("estoque.insuficiente", new
            {
                PedidoId = pedidoId,
                ProdutoId = produtoId,
                QuantidadeSolicitada = quantidade,
                QuantidadeDisponivel = item?.QuantidadeDisponivel ?? 0
            }, ct);
            return;
        }

        await _gateway.Update(item, ct);

        await _eventPublisher.PublishAsync("estoque.reservado", new
        {
            PedidoId = pedidoId,
            ProdutoId = produtoId,
            Quantidade = quantidade
        }, ct);
    }
}
