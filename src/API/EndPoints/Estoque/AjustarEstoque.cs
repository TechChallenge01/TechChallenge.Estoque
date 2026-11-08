using API.Extensions;
using Application.Controllers.Estoque;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Estoque.Request;

namespace API.EndPoints.Estoque;

public class AjustarEstoque : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/estoque/{produtoId:guid}/ajustar", async (
            Guid produtoId,
            [FromBody] AjustarEstoqueRequestDTO request,
            HttpContext ctx,
            IItemEstoqueDataSource dataSource,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new EstoqueController(dataSource);
            var result = await controller.AjustarEstoque(produtoId, request, idUsuario, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Estoque")
        .WithSummary("Ajusta a quantidade disponível de um item");
    }
}
