using Application.Interfaces;
using MongoDB.Driver;
using Shared.DTOs.Estoque.Input;

namespace Infra.DataSources;

public class ItemEstoqueDataSource : IItemEstoqueDataSource
{
    private readonly IMongoCollection<ItemEstoqueInputDTO> _collection;

    public ItemEstoqueDataSource(IMongoDatabase database)
        => _collection = database.GetCollection<ItemEstoqueInputDTO>("itens_estoque");

    public async Task<ItemEstoqueInputDTO?> GetById(Guid id, CancellationToken ct)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<ItemEstoqueInputDTO?> GetByProdutoId(Guid produtoId, CancellationToken ct)
        => await _collection.Find(x => x.ProdutoId == produtoId).FirstOrDefaultAsync(ct);

    public async Task Create(ItemEstoqueInputDTO dto, CancellationToken ct)
        => await _collection.InsertOneAsync(dto, cancellationToken: ct);

    public async Task Update(ItemEstoqueInputDTO dto, CancellationToken ct)
        => await _collection.ReplaceOneAsync(x => x.Id == dto.Id, dto, cancellationToken: ct);
}
