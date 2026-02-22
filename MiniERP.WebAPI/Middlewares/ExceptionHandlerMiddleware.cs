using System.Net;
using System.Text.Json;
using FluentValidation; // Bunu eklemeyi unutma
using MiniERP.Domain.Common;

namespace MiniERP.WebAPI.Middlewares;

public sealed class ExceptionHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // 1. Gelen hata bir ValidationException ise:
        if (exception is ValidationException validationException)
        {
            // Kullanıcı hatası olduğu için 422 (Unprocessable Entity) veya 400 dönüyoruz
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

            // FluentValidation içindeki hataları temiz bir listeye çeviriyoruz
            var validationErrors = validationException.Errors.Select(e => e.ErrorMessage).ToList();

            var validationResult = Result<object>.Failure("Doğrulama (Validation) hatası.", validationErrors);

            await context.Response.WriteAsync(JsonSerializer.Serialize(validationResult));
            return; // İşlemi burada kes
        }

        // 2. Gelen hata normal bir sistem çökmesi ise (Eski kodumuz):
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 hatası

        var errorResult = Result<object>.Failure(
            "Sistemde beklenmedik bir hata oluştu.",
            new List<string> { exception.Message }
        );

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResult));
    }
}