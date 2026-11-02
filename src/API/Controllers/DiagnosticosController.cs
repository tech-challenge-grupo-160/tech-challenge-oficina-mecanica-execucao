using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Requests.Diagnosticos;
using Fiap.TechChallenge.OficinaMecanica.Execucao.API.Responses.Diagnosticos;
using Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Diagnosticos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.API.Controllers;

[ApiController]
[Route("api/v1/diagnosticos")]
public sealed class DiagnosticosController(
    ISender sender,
    IWebHostEnvironment environment) : ControllerBase
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
        if (!environment.IsDevelopment() && !environment.IsEnvironment("IntegrationTesting"))
            return NotFound();

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

    [HttpPut("ordem-servico/{ordemServicoId:int}/resultado")]
    [ProducesResponseType<ResultadoDiagnosticoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResultadoDiagnosticoResponse>> RegistrarResultado(
        int ordemServicoId,
        RegistrarResultadoDiagnosticoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegistrarResultadoDiagnosticoCommand(
            ordemServicoId,
            request.ParecerTecnico,
            request.ServicoIds,
            (request.Pecas ?? []).Select(item =>
                new PecaResultadoDiagnosticoCommand(item.PecaId, item.Quantidade)).ToArray()),
            cancellationToken);

        return Ok(Mapear(result));
    }

    [HttpGet("ordem-servico/{ordemServicoId:int}/resultado")]
    [ProducesResponseType<ResultadoDiagnosticoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultadoDiagnosticoResponse>> ObterResultado(
        int ordemServicoId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ObterResultadoDiagnosticoPorOrdemQuery(ordemServicoId),
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

    private static ResultadoDiagnosticoResponse Mapear(ResultadoDiagnosticoResult result) => new(
        result.DiagnosticoId,
        result.OrdemServicoId,
        result.ParecerTecnico,
        result.ServicoIds,
        result.Pecas
            .Select(item => new PecaResultadoDiagnosticoResponse(item.PecaId, item.Quantidade))
            .ToArray(),
        result.AtualizadoEm,
        result.Versao);
}
