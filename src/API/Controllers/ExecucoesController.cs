using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Execucoes;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Execucoes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Controllers;

[ApiController]
[Route("api/v1/execucoes")]
public sealed class ExecucoesController(ISender sender) : ControllerBase
{
    [HttpGet("fila")]
    [ProducesResponseType<IReadOnlyCollection<ExecucaoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ExecucaoResponse>>> ListarFila(
        CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarFilaExecucaoQuery(), cancellationToken)).Select(Mapear));

    [HttpGet("ordem-servico/{ordemServicoId:int}")]
    [ProducesResponseType<ExecucaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecucaoResponse>> ObterPorOrdem(
        int ordemServicoId,
        CancellationToken cancellationToken) =>
        Ok(Mapear(await sender.Send(
            new ObterExecucaoPorOrdemQuery(ordemServicoId), cancellationToken)));

    [HttpPatch("ordem-servico/{ordemServicoId:int}/finalizar")]
    [ProducesResponseType<ExecucaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecucaoResponse>> Finalizar(
        int ordemServicoId,
        CancellationToken cancellationToken) =>
        Ok(Mapear(await sender.Send(
            new FinalizarExecucaoCommand(ordemServicoId), cancellationToken)));

    private static ExecucaoResponse Mapear(ExecucaoResult result) => new(
        result.Id,
        result.OrdemServicoId,
        result.DiagnosticoId,
        result.Status,
        result.IniciadaEm,
        result.FinalizadaEm);
}
