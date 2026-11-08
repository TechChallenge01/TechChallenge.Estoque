using Domain.BaseEntity;

namespace Domain.Aggregates;

public class ItemEstoque : Base
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public int QuantidadeDisponivel { get; private set; }
    public int QuantidadeReservada { get; private set; }

    public ItemEstoque(Guid produtoId, int quantidadeInicial, Guid idUsuario)
        : base(idUsuario, DateTime.UtcNow)
    {
        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        QuantidadeDisponivel = quantidadeInicial;
        QuantidadeReservada = 0;
    }

    public ItemEstoque(Guid id, Guid produtoId, int quantidadeDisponivel, int quantidadeReservada,
        Guid idUsuarioCriacao, DateTime dataCriacao,
        Guid? idUsuarioAtualizacao, DateTime? dataAtualizacao, bool ativo)
        : base(idUsuarioCriacao, dataCriacao, idUsuarioAtualizacao, dataAtualizacao)
    {
        Id = id;
        ProdutoId = produtoId;
        QuantidadeDisponivel = quantidadeDisponivel;
        QuantidadeReservada = quantidadeReservada;
        Ativo = ativo;
    }

    public bool Reservar(int quantidade)
    {
        if (QuantidadeDisponivel < quantidade)
            return false;

        QuantidadeDisponivel -= quantidade;
        QuantidadeReservada += quantidade;
        return true;
    }

    public void Liberar(int quantidade)
    {
        QuantidadeReservada = Math.Max(0, QuantidadeReservada - quantidade);
        QuantidadeDisponivel += quantidade;
    }

    public void AjustarEstoque(int novaQuantidade)
        => QuantidadeDisponivel = novaQuantidade;
}
