using Domain.Aggregates;
using FluentAssertions;

namespace UnitTests.Domain;

public class ItemEstoqueTests
{
    private static ItemEstoque CriarItem(int quantidade = 10)
        => new(Guid.NewGuid(), quantidade, Guid.NewGuid());

    [Fact]
    public void CriarItemEstoque_DeveInicializarQuantidades()
    {
        var item = CriarItem(20);

        item.QuantidadeDisponivel.Should().Be(20);
        item.QuantidadeReservada.Should().Be(0);
    }

    [Fact]
    public void CriarItemEstoque_DeveGerarId()
    {
        var item = CriarItem();

        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Reservar_QuandoQuantidadeSuficiente_DeveRetornarTrue()
    {
        var item = CriarItem(10);

        var resultado = item.Reservar(5);

        resultado.Should().BeTrue();
    }

    [Fact]
    public void Reservar_QuandoQuantidadeInsuficiente_DeveRetornarFalse()
    {
        var item = CriarItem(3);

        var resultado = item.Reservar(5);

        resultado.Should().BeFalse();
    }

    [Fact]
    public void Reservar_DeveDecrementarDisponivel()
    {
        var item = CriarItem(10);

        item.Reservar(4);

        item.QuantidadeDisponivel.Should().Be(6);
    }

    [Fact]
    public void Reservar_DeveIncrementarReservada()
    {
        var item = CriarItem(10);

        item.Reservar(4);

        item.QuantidadeReservada.Should().Be(4);
    }

    [Fact]
    public void Reservar_QuandoExatamenteDisponivel_DeveRetornarTrue()
    {
        var item = CriarItem(5);

        var resultado = item.Reservar(5);

        resultado.Should().BeTrue();
        item.QuantidadeDisponivel.Should().Be(0);
        item.QuantidadeReservada.Should().Be(5);
    }

    [Fact]
    public void Liberar_DeveIncrementarDisponivel()
    {
        var item = CriarItem(10);
        item.Reservar(5);

        item.Liberar(3);

        item.QuantidadeDisponivel.Should().Be(8);
    }

    [Fact]
    public void Liberar_DeveDecrementarReservada()
    {
        var item = CriarItem(10);
        item.Reservar(5);

        item.Liberar(3);

        item.QuantidadeReservada.Should().Be(2);
    }

    [Fact]
    public void Liberar_NaoPodeTeReservadaNegativa()
    {
        var item = CriarItem(10);
        item.Reservar(2);

        item.Liberar(10);

        item.QuantidadeReservada.Should().Be(0);
    }

    [Fact]
    public void AjustarEstoque_DeveModificarDisponivel()
    {
        var item = CriarItem(10);

        item.AjustarEstoque(50);

        item.QuantidadeDisponivel.Should().Be(50);
    }
}
