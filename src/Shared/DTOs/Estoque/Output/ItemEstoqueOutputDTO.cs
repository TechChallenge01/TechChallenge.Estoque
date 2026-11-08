namespace Shared.DTOs.Estoque.Output;

public class ItemEstoqueOutputDTO
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public int QuantidadeReservada { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
