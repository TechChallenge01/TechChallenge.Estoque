using Application.Gateways.Estoque;
using Application.Interfaces;
using Application.UseCases.Estoque;
using FluentAssertions;
using Moq;
using Shared.DTOs.Estoque.Input;
using System.Net;

namespace UnitTests.Application.UseCases;

public class GetItemEstoquePorProdutoUseCaseTests
{
    private readonly Mock<IItemEstoqueDataSource> _mockDS = new();

    private GetItemEstoquePorProdutoUseCase CriarUseCase()
        => GetItemEstoquePorProdutoUseCase.Create(ItemEstoqueGateway.Create(_mockDS.Object));

    [Fact]
    public async Task Run_QuandoItemExiste_DeveRetornarOk()
    {
        var dto = new ItemEstoqueInputDTO
        {
            Id = Guid.NewGuid(), ProdutoId = Guid.NewGuid(), QuantidadeDisponivel = 5,
            QuantidadeReservada = 2, IdUsuarioCriacao = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow, Ativo = true
        };
        _mockDS.Setup(x => x.GetByProdutoId(dto.ProdutoId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await CriarUseCase().Run(dto.ProdutoId, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Run_QuandoItemNaoExiste_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetByProdutoId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueInputDTO?)null);

        var result = await CriarUseCase().Run(Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
