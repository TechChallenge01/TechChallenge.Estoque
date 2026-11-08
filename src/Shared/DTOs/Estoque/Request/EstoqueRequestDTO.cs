namespace Shared.DTOs.Estoque.Request;

public class AdicionarItemEstoqueRequestDTO
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}

public class AjustarEstoqueRequestDTO
{
    public int NovaQuantidade { get; set; }
}
