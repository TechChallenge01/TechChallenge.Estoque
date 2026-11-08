using Application.Interfaces;
using Domain.Aggregates;
using Shared.DTOs.Estoque.Input;

namespace Application.Gateways.Estoque;

public class ItemEstoqueGateway
{
    private readonly IItemEstoqueDataSource _dataSource;

    private ItemEstoqueGateway(IItemEstoqueDataSource dataSource) => _dataSource = dataSource;

    public static ItemEstoqueGateway Create(IItemEstoqueDataSource dataSource) => new(dataSource);

    public async Task<ItemEstoque?> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _dataSource.GetById(id, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task<ItemEstoque?> GetByProdutoId(Guid produtoId, CancellationToken ct)
    {
        var dto = await _dataSource.GetByProdutoId(produtoId, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task Create(ItemEstoque item, CancellationToken ct)
        => await _dataSource.Create(Serializar(item), ct);

    public async Task Update(ItemEstoque item, CancellationToken ct)
        => await _dataSource.Update(Serializar(item), ct);

    private static ItemEstoque Reconstituir(ItemEstoqueInputDTO dto) =>
        new(dto.Id, dto.ProdutoId, dto.QuantidadeDisponivel, dto.QuantidadeReservada,
            dto.IdUsuarioCriacao, dto.DataCriacao,
            dto.IdUsuarioAtualizacao, dto.DataAtualizacao, dto.Ativo);

    private static ItemEstoqueInputDTO Serializar(ItemEstoque i) => new()
    {
        Id = i.Id,
        ProdutoId = i.ProdutoId,
        QuantidadeDisponivel = i.QuantidadeDisponivel,
        QuantidadeReservada = i.QuantidadeReservada,
        IdUsuarioCriacao = i.IdUsuarioCriacao,
        DataCriacao = i.DataCriacao,
        IdUsuarioAtualizacao = i.IdUsuarioAtualizacao,
        DataAtualizacao = i.DataAtualizacao,
        Ativo = i.Ativo
    };
}
