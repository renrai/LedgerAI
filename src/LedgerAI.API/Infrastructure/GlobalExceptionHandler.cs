using FluentValidation;
using LedgerAI.Application.Services;
using LedgerAI.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LedgerAI.API.Infrastructure;

/// <summary>
/// Converte exceções conhecidas em respostas ProblemDetails (RFC 9457) com o status HTTP correto.
/// Exceções desconhecidas viram 500 sem vazar detalhes internos.
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, title, errors) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, "Dados inválidos.",
                ve.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            DomainException => (StatusCodes.Status400BadRequest, exception.Message, null),
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message, null),
            UnauthorizedException or UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, exception.Message, null),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Requisição cancelada.", null),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.", null)
        };

        if (status >= 500)
            logger.LogError(exception, "Erro não tratado em {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("Erro {Status} em {Method} {Path}: {Message}", status, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.io/{status}"
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
