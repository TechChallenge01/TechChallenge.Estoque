using Application.Gateways.Estoque;
using Application.Interfaces;
using Application.UseCases.Estoque;
using FluentAssertions;
using Moq;
using Shared.DTOs.Estoque.Input;
using Shared.DTOs.Estoque.Request;
using System.Net;

namespace UnitTests.Application.UseCases;

public class AdicionarItemEstoqueUseCaseTests
{
    private readonly Mock<IItemEstoqueDataSource> _mockDS = new();

    private AdicionarItemEstoqueUseCase CriarUseCase()
        => AdicionarItemEstoqueUseCase.Create(ItemEstoqueGateway.Create(_mockDS.Object));

    private static ItemEstoqueInputDTO ItemDTO(int disponivel = 10) => new()
    {
        Id = Guid.NewGuid(), ProdutoId = Guid.NewGuid(), QuantidadeDisponivel = disponivel,
        QuantidadeReservada = 0, IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoItemNaoExiste_DeveCriarNovoItem()
    {
        _mockDS.Setup(x => x.GetByProdutoId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueInputDTO?)null);
        _mockDS.Setup(x => x.Create(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new AdicionarItemEstoqueRequestDTO { ProdutoId = Guid.NewGuid(), Quantidade = 20 };
        var result = await CriarUseCase().Run(request, Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        _mockDS.Verify(x => x.Create(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoItemExiste_DeveAtualizarQuantidade()
    {
        var dto = ItemDTO(10);
        _mockDS.Setup(x => x.GetByProdutoId(dto.ProdutoId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new AdicionarItemEstoqueRequestDTO { ProdutoId = dto.ProdutoId, Quantidade = 5 };
        var result = await CriarUseCase().Run(request, Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockDS.Verify(x => x.Update(It.Is<ItemEstoqueInputDTO>(d => d.QuantidadeDisponivel == 15), It.IsAny<CancellationToken>()), Times.Once);
    }
}
