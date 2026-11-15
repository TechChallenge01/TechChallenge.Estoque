using Application.Gateways.Estoque;
using Application.Interfaces;
using Application.UseCases.Estoque;
using FluentAssertions;
using Moq;
using Shared.DTOs.Estoque.Input;

namespace UnitTests.Application.UseCases;

public class ReservarEstoqueUseCaseTests
{
    private readonly Mock<IItemEstoqueDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private ReservarEstoqueUseCase CriarUseCase()
        => ReservarEstoqueUseCase.Create(ItemEstoqueGateway.Create(_mockDS.Object), _mockPublisher.Object);

    [Fact]
    public async Task Run_QuandoEstoqueSuficiente_DevePublicarEstoqueReservado()
    {
        var dto = new ItemEstoqueInputDTO
        {
            Id = Guid.NewGuid(), ProdutoId = Guid.NewGuid(), QuantidadeDisponivel = 10,
            QuantidadeReservada = 0, IdUsuarioCriacao = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow, Ativo = true
        };
        _mockDS.Setup(x => x.GetByProdutoId(dto.ProdutoId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<ItemEstoqueInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CriarUseCase().Run(Guid.NewGuid(), dto.ProdutoId, 5, CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("estoque.reservado", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoEstoqueInsuficiente_DevePublicarEstoqueInsuficiente()
    {
        var dto = new ItemEstoqueInputDTO
        {
            Id = Guid.NewGuid(), ProdutoId = Guid.NewGuid(), QuantidadeDisponivel = 2,
            QuantidadeReservada = 0, IdUsuarioCriacao = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow, Ativo = true
        };
        _mockDS.Setup(x => x.GetByProdutoId(dto.ProdutoId, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CriarUseCase().Run(Guid.NewGuid(), dto.ProdutoId, 10, CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("estoque.insuficiente", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoItemNaoEncontrado_DevePublicarEstoqueInsuficiente()
    {
        _mockDS.Setup(x => x.GetByProdutoId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemEstoqueInputDTO?)null);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CriarUseCase().Run(Guid.NewGuid(), Guid.NewGuid(), 1, CancellationToken.None);

        _mockPublisher.Verify(x => x.PublishAsync("estoque.insuficiente", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
