using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReadX.Api.Exceptions;
using ReadX.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReadX.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly ILocalizationService _localizationService;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, ILocalizationService localizationService)
    {
        _next = next;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            await HandleExceptionAsync(context, ex.StatusCode, ex.ErrorCode, _localizationService.GetMessage(ex.ErrorCode));
        }
        catch (FluentValidation.ValidationException ex)
        {
            var firstError = ex.Errors.FirstOrDefault();
            var errorCode = firstError?.ErrorCode ?? "ValidationFailed";
            var message = firstError?.ErrorMessage ?? _localizationService.GetMessage(errorCode);
            
            await HandleExceptionAsync(context, StatusCodes.Status422UnprocessableEntity, errorCode, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "InternalServerError", "An unexpected error occurred.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new
        {
            error = new
            {
                code = errorCode,
                message = message
            }
        });

        return context.Response.WriteAsync(result);
    }
}
