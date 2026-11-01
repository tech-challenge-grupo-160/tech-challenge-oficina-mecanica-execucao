using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Catalogo;
using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Catalogo;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Catalogo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Controllers;

[ApiController]
[Route("api/v1/catalogo")]
public sealed class CatalogoController(ISender sender) : ControllerBase
{
    [HttpPost("itens/consultar")]
    [ProducesResponseType<CatalogoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CatalogoResponse>> ConsultarItens(
        ConsultarCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ConsultarCatalogoQuery(request.ServicoIds ?? [], request.PecaIds ?? []),
            cancellationToken);

        return Ok(new CatalogoResponse(
            result.Servicos
                .Select(item => new ServicoCatalogoResponse(
                    item.Id, item.Nome, item.Preco, item.TempoEstimado))
                .ToArray(),
            result.Pecas
                .Select(item => new PecaCatalogoResponse(item.Id, item.Nome, item.Preco))
                .ToArray(),
            result.ServicoIdsNaoEncontrados,
            result.PecaIdsNaoEncontrados));
    }
}
