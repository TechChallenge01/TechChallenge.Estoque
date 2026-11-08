using Application.Gateways.Estoque;
using Application.Presenters.Estoque;
using Shared.Result;

namespace Application.UseCases.Estoque;

public class GetItemEstoquePorProdutoUseCase
{
    private readonly ItemEstoqueGateway _gateway;
    private readonly ItemEstoquePresenter _presenter = new("Estoque encontrado.");

    private GetItemEstoquePorProdutoUseCase(ItemEstoqueGateway gateway) => _gateway = gateway;

    public static GetItemEstoquePorProdutoUseCase Create(ItemEstoqueGateway gateway) => new(gateway);

    public async Task<ICommandResult> Run(Guid produtoId, CancellationToken ct)
    {
        var item = await _gateway.GetByProdutoId(produtoId, ct);
        return item is null ? _presenter.NotFound("Estoque não encontrado para este produto.") : _presenter.TransformObject(item);
    }
}
