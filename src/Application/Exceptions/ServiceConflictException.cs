using System.Net;

namespace Fiap.TechChallenge.OficinaMecanica.Execucao.Application.Exceptions;

public sealed class ServiceConflictException(string message)
    : InvalidOperationException(message), IServiceExceptionContract
{
    public int StatusCode => (int)HttpStatusCode.Conflict;
}
