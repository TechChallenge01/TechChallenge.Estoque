using Application.Gateways.Estoque;
using Application.Presenters.Estoque;
using Domain.Aggregates;
using Shared.DTOs.Estoque.Request;
using Shared.Result;

namespace Application.UseCases.Estoque;

public class AdicionarItemEstoqueUseCase
{
    private readonly ItemEstoqueGateway _gateway;
    private readonly ItemEstoquePresenter _presenter = new("Item de estoque adicionado/atualizado com sucesso.");

    private AdicionarItemEstoqueUseCase(ItemEstoqueGateway gateway) => _gateway = gateway;

    public static AdicionarItemEstoqueUseCase Create(ItemEstoqueGateway gateway) => new(gateway);

    public async Task<ICommandResult> Run(AdicionarItemEstoqueRequestDTO request, Guid idUsuario, CancellationToken ct)
    {
        var existente = await _gateway.GetByProdutoId(request.ProdutoId, ct);

        if (existente is not null)
        {
            existente.AjustarEstoque(existente.QuantidadeDisponivel + request.Quantidade);
            existente.RastrearAlteracao(idUsuario, DateTime.UtcNow);
            await _gateway.Update(existente, ct);
            return _presenter.TransformObject(existente);
        }

        var item = new ItemEstoque(request.ProdutoId, request.Quantidade, idUsuario);
        await _gateway.Create(item, ct);
        return _presenter.Created(item.Id);
    }
}
