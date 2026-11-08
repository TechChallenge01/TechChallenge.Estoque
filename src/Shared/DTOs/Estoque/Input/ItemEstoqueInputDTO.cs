namespace Shared.DTOs.Estoque.Input;

public class ItemEstoqueInputDTO
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public int QuantidadeReservada { get; set; }
    public Guid IdUsuarioCriacao { get; set; }
    public DateTime DataCriacao { get; set; }
    public Guid? IdUsuarioAtualizacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }
}
