using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MiniERP.Persistence.Context;

namespace MiniERP.WebAPI.Middlewares
{
    public sealed class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // 1. Authorization header'ından token'ı ayıkla
            string token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                // 2. Veritabanında bu token "çıkış yapmış" mı diye bak
                bool isBlacklisted = await dbContext.InvalidTokens.AnyAsync(x => x.Token == token);

                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Oturum sonlandırıldı, lütfen tekrar giriş yapın." });
                    return;
                }
            }

            await _next(context);
        }
    }
}
