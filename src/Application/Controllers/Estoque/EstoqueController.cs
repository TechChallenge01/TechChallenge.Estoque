using Application.Gateways.Estoque;
using Application.Interfaces;
using Application.UseCases.Estoque;
using Shared.DTOs.Estoque.Request;
using Shared.Result;

namespace Application.Controllers.Estoque;

public class EstoqueController
{
    private readonly IItemEstoqueDataSource _dataSource;

    public EstoqueController(IItemEstoqueDataSource dataSource) => _dataSource = dataSource;

    public Task<ICommandResult> AdicionarItemEstoque(AdicionarItemEstoqueRequestDTO request, Guid idUsuario, CancellationToken ct)
        => AdicionarItemEstoqueUseCase.Create(ItemEstoqueGateway.Create(_dataSource)).Run(request, idUsuario, ct);

    public Task<ICommandResult> AjustarEstoque(Guid produtoId, AjustarEstoqueRequestDTO request, Guid idUsuario, CancellationToken ct)
        => AjustarEstoqueUseCase.Create(ItemEstoqueGateway.Create(_dataSource)).Run(produtoId, request, idUsuario, ct);

    public Task<ICommandResult> GetByProdutoId(Guid produtoId, CancellationToken ct)
        => GetItemEstoquePorProdutoUseCase.Create(ItemEstoqueGateway.Create(_dataSource)).Run(produtoId, ct);

    public Task ReservarEstoque(Guid pedidoId, Guid produtoId, int quantidade, IEventPublisher eventPublisher, CancellationToken ct)
        => ReservarEstoqueUseCase.Create(ItemEstoqueGateway.Create(_dataSource), eventPublisher).Run(pedidoId, produtoId, quantidade, ct);

    public Task LiberarEstoque(Guid pedidoId, Guid produtoId, int quantidade, IEventPublisher eventPublisher, CancellationToken ct)
        => LiberarEstoqueUseCase.Create(ItemEstoqueGateway.Create(_dataSource), eventPublisher).Run(pedidoId, produtoId, quantidade, ct);
}
