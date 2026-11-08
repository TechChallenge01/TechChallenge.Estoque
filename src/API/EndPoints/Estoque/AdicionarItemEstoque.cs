using API.Extensions;
using Application.Controllers.Estoque;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Estoque.Request;

namespace API.EndPoints.Estoque;

public class AdicionarItemEstoque : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/estoque", async (
            [FromBody] AdicionarItemEstoqueRequestDTO request,
            HttpContext ctx,
            IItemEstoqueDataSource dataSource,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new EstoqueController(dataSource);
            var result = await controller.AdicionarItemEstoque(request, idUsuario, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Estoque")
        .WithSummary("Adiciona ou incrementa item no estoque");
    }
}
