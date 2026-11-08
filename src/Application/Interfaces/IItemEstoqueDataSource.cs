using Shared.DTOs.Estoque.Input;

namespace Application.Interfaces;

public interface IItemEstoqueDataSource
{
    Task<ItemEstoqueInputDTO?> GetById(Guid id, CancellationToken ct);
    Task<ItemEstoqueInputDTO?> GetByProdutoId(Guid produtoId, CancellationToken ct);
    Task Create(ItemEstoqueInputDTO dto, CancellationToken ct);
    Task Update(ItemEstoqueInputDTO dto, CancellationToken ct);
}
