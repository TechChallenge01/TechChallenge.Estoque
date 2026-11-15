using Application.Gateways.Estoque;
using Application.Interfaces;
using Application.UseCases.Estoque;
using FluentAssertions;
using Moq;
using Shared.DTOs.Estoque.Input;

namespace UnitTests.Application.UseCases;

public class LiberarEstoqueUseCaseTests
{
    private readonly Mock<IItemEstoqueDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private LiberarEstoqueUseCase CriarUseCase()
        => LiberarEstoqueUseCase.Create(ItemEstoqueGateway.Create(_mockDS.Object), _mockPublisher.Object);

    [Fact]
    public async Task Run_QuandoItemExiste_DeveLiberarEPublicar()
    {
        var dto = new ItemEstoqueInputDTO
        {
            Id = Guid.NewGuid(), ProdutoId = Guid.NewGuid(), QuantidadeDisponivel = 5,
            QuantidadeReservada = 3, IdUsuarioCriacao = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow, Ativo = true
        };
        _mockDS.Setup(x => x.GetByProdutoId(dto.ProdutoId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CriarUseCase().Run(Guid.NewGuid(), dto.ProdutoId, 2, CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("estoque.liberado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockDS.Verify(x => x.Update(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoItemNaoEncontrado_NaoDeveFazerNada()
    {
        _mockDS.Setup(x => x.GetByProdutoId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueInputDTO?)null);

        await CriarUseCase().Run(Guid.NewGuid(), Guid.NewGuid(), 5, CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
