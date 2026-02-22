using System.Net;
using System.Text.Json;
using MiniERP.Domain.Common; // Result sınıfın burada olduğu için

namespace MiniERP.WebAPI.Middlewares;

public sealed class ExceptionHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            // İsteği bir sonraki adıma (Controller'a) gönderiyoruz
            await next(context);
        }
        catch (Exception ex)
        {
            // Eğer yolda bir hata patlarsa burası yakalıyor!
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 hatası

        var response = Result<object>.Failure(
            "Sistemde beklenmedik bir hata oluştu.",
            new List<string> { exception.Message } // Geliştirme aşamasında mesajı görelim
        );

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}