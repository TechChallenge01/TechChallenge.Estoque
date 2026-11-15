using Application.Presenters.Estoque;
using Domain.Aggregates;
using FluentAssertions;
using System.Net;

namespace UnitTests.Application.Presenters;

public class ItemEstoquePresenterTests
{
    private static ItemEstoque CriarItem() => new(Guid.NewGuid(), 10, Guid.NewGuid());
    private static ItemEstoquePresenter CriarPresenter() => new("Operação realizada");

    [Fact]
    public void TransformObject_DeveRetornarStatusOK()
    {
        var result = CriarPresenter().TransformObject(CriarItem());

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data.QuantidadeDisponivel.Should().Be(10);
    }

    [Fact]
    public void Created_DeveRetornarStatusCreated()
    {
        var id = Guid.NewGuid();
        var result = CriarPresenter().Created(id);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().Be(id);
    }

    [Fact]
    public void NotFound_DeveRetornarStatusNotFound()
    {
        var result = CriarPresenter().NotFound("não encontrado");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("não encontrado");
    }

    [Fact]
    public void NotFound_SemMensagem_DeveUsarMensagemPadrao()
    {
        var result = CriarPresenter().NotFound();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Operação realizada");
    }

    [Fact]
    public void BadRequest_DeveRetornarStatusBadRequest()
    {
        var result = CriarPresenter().BadRequest("erro");
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
