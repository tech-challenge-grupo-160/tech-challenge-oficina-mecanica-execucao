using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Estoque;
using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Estoque;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Estoque;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Controllers;

[ApiController]
[Route("api/v1/estoque")]
public sealed class EstoqueController(ISender sender) : ControllerBase
{
    [HttpPost("pecas/{pecaId:int}/reposicoes")]
    [ProducesResponseType<ReposicaoEstoqueResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReposicaoEstoqueResponse>> Repor(
        int pecaId,
        ReporEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(
            new ReporEstoqueCommand(pecaId, request.Quantidade),
            cancellationToken);
        return Ok(new ReposicaoEstoqueResponse(
            resultado.PecaId,
            resultado.QuantidadeReposta,
            resultado.QuantidadeEstoque,
            resultado.OrdensServicoLiberadas));
    }
}
