using API.Extensions;
using Application.Controllers.Estoque;
using Application.Interfaces;

namespace API.EndPoints.Estoque;

public class GetItemEstoquePorProduto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/estoque/{produtoId:guid}", async (
            Guid produtoId,
            IItemEstoqueDataSource dataSource,
            CancellationToken ct) =>
        {
            var controller = new EstoqueController(dataSource);
            var result = await controller.GetByProdutoId(produtoId, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Estoque")
        .WithSummary("Consulta o estoque de um produto");
    }
}
