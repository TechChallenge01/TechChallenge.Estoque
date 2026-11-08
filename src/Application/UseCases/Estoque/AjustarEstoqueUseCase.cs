using Application.Gateways.Estoque;
using Application.Presenters.Estoque;
using Shared.DTOs.Estoque.Request;
using Shared.Result;

namespace Application.UseCases.Estoque;

public class AjustarEstoqueUseCase
{
    private readonly ItemEstoqueGateway _gateway;
    private readonly ItemEstoquePresenter _presenter = new("Estoque ajustado com sucesso.");

    private AjustarEstoqueUseCase(ItemEstoqueGateway gateway) => _gateway = gateway;

    public static AjustarEstoqueUseCase Create(ItemEstoqueGateway gateway) => new(gateway);

    public async Task<ICommandResult> Run(Guid produtoId, AjustarEstoqueRequestDTO request, Guid idUsuario, CancellationToken ct)
    {
        var item = await _gateway.GetByProdutoId(produtoId, ct);
        if (item is null)
            return _presenter.NotFound("Item de estoque não encontrado para este produto.");

        item.AjustarEstoque(request.NovaQuantidade);
        item.RastrearAlteracao(idUsuario, DateTime.UtcNow);
        await _gateway.Update(item, ct);

        return _presenter.TransformObject(item);
    }
}
