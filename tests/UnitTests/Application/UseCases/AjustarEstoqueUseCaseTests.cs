using Application.Gateways.Estoque;
using Application.Interfaces;
using Application.UseCases.Estoque;
using FluentAssertions;
using Moq;
using Shared.DTOs.Estoque.Input;
using Shared.DTOs.Estoque.Request;
using System.Net;

namespace UnitTests.Application.UseCases;

public class AjustarEstoqueUseCaseTests
{
    private readonly Mock<IItemEstoqueDataSource> _mockDS = new();

    private AjustarEstoqueUseCase CriarUseCase()
        => AjustarEstoqueUseCase.Create(ItemEstoqueGateway.Create(_mockDS.Object));

    private static ItemEstoqueInputDTO ItemDTO() => new()
    {
        Id = Guid.NewGuid(), ProdutoId = Guid.NewGuid(), QuantidadeDisponivel = 10,
        QuantidadeReservada = 0, IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoItemExiste_DeveAjustarQuantidade()
    {
        var dto = ItemDTO();
        _mockDS.Setup(x => x.GetByProdutoId(dto.ProdutoId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new AjustarEstoqueRequestDTO { NovaQuantidade = 50 };
        var result = await CriarUseCase().Run(dto.ProdutoId, request, Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockDS.Verify(x => x.Update(It.Is<ItemEstoqueInputDTO>(d => d.QuantidadeDisponivel == 50), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoItemNaoExiste_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetByProdutoId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueInputDTO?)null);

        var request = new AjustarEstoqueRequestDTO { NovaQuantidade = 100 };
        var result = await CriarUseCase().Run(Guid.NewGuid(), request, Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
