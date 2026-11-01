using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Diagnosticos;
using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Diagnosticos;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Controllers;

[ApiController]
[Route("api/v1/diagnosticos")]
public sealed class DiagnosticosController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<DiagnosticoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<DiagnosticoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DiagnosticoResponse>> Iniciar(
        IniciarDiagnosticoRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? chaveIdempotencia,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new IniciarDiagnosticoCommand(
            request.OrdemServicoId,
            request.VeiculoId,
            request.DescricaoSolicitacao,
            request.ServicoIdsSolicitados,
            request.PecaIdsSolicitadas,
            chaveIdempotencia ?? string.Empty), cancellationToken);
        var response = Mapear(result);

        return result.Criado
            ? CreatedAtAction(
                nameof(ObterPorOrdem),
                new { ordemServicoId = result.OrdemServicoId },
                response)
            : Ok(response);
    }

    [HttpGet("ordem-servico/{ordemServicoId:int}")]
    [ProducesResponseType<DiagnosticoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiagnosticoResponse>> ObterPorOrdem(
        int ordemServicoId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ObterDiagnosticoPorOrdemQuery(ordemServicoId),
            cancellationToken);

        return Ok(Mapear(result));
    }

    private static DiagnosticoResponse Mapear(IniciarDiagnosticoResult result) => new(
        result.Id,
        result.OrdemServicoId,
        result.VeiculoId,
        result.DescricaoSolicitacao,
        result.ServicoIdsSolicitados,
        result.PecaIdsSolicitadas,
        result.Status,
        result.IniciadoEm);
}
