using System.Net;
using Domain.Aggregates;
using Shared.DTOs.Estoque.Output;
using Shared.Result;

namespace Application.Presenters.Estoque;

public class ItemEstoquePresenter
{
    private readonly string _message;

    public ItemEstoquePresenter(string message) => _message = message;

    public ICommandResult<ItemEstoqueOutputDTO> TransformObject(ItemEstoque item)
        => new CommandResult<ItemEstoqueOutputDTO>(_message, HttpStatusCode.OK, Transform(item));

    public ICommandResult<T> Created<T>(T data)
        => new CommandResult<T>(_message, HttpStatusCode.Created, data);

    public ICommandResult NotFound(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.NotFound);

    public ICommandResult BadRequest(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.BadRequest);

    private static ItemEstoqueOutputDTO Transform(ItemEstoque i) => new()
    {
        Id = i.Id,
        ProdutoId = i.ProdutoId,
        QuantidadeDisponivel = i.QuantidadeDisponivel,
        QuantidadeReservada = i.QuantidadeReservada,
        Ativo = i.Ativo,
        DataCriacao = i.DataCriacao,
        DataAtualizacao = i.DataAtualizacao
    };
}
