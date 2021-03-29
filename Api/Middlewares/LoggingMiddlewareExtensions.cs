namespace Api.Middlewares
{
    using Microsoft.AspNetCore.Builder;

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LoggingMiddleware>();

            return app;
        }
    }
}