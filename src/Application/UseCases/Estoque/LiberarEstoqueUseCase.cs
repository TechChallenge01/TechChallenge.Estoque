using Application.Gateways.Estoque;
using Application.Interfaces;

namespace Application.UseCases.Estoque;

public class LiberarEstoqueUseCase
{
    private readonly ItemEstoqueGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private LiberarEstoqueUseCase(ItemEstoqueGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static LiberarEstoqueUseCase Create(ItemEstoqueGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(Guid pedidoId, Guid produtoId, int quantidade, CancellationToken ct)
    {
        var item = await _gateway.GetByProdutoId(produtoId, ct);
        if (item is null) return;

        item.Liberar(quantidade);
        await _gateway.Update(item, ct);

        await _eventPublisher.PublishAsync("estoque.liberado", new
        {
            PedidoId = pedidoId,
            ProdutoId = produtoId,
            Quantidade = quantidade
        }, ct);
    }
}
