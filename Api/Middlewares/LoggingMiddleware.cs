namespace Api.Middlewares
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IO;
    using Serilog;

    internal class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public LoggingMiddleware(RequestDelegate next)
        {
            this._next = next;
            this._recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            await using var requestStream = this._recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            Log.Information($"HTTP Request Information: {Environment.NewLine}" +
                            $"TraceId: {Activity.Current.Id} " +
                            $"Schema: {context.Request.Scheme} " +
                            $"Host: {context.Request.Host} " +
                            $"Path: {context.Request.Path} " +
                            $"QueryString: {context.Request.QueryString} " +
                            $"Request Body: {ReadStreamInChunks(requestStream)}");

            context.Request.Body.Position = 0;
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;

            stream.Seek(0, SeekOrigin.Begin);

            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);

            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            await using var responseBody = this._recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            await this._next(context);

            string text = string.Empty;
            if (context.Request.Path.Value != "/swagger"
                && context.Request.Path.Value != "/swagger/index.html"
                && context.Request.Path.Value.Contains("favicon") == false
                && context.Request.Path.Value.Contains("swagger") == false)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                text = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
            }

            Log.Information($"HTTP Response Information: {Environment.NewLine}" +
                            $"TraceId: {Activity.Current.Id} " +
                            $"Schema: {context.Request.Scheme} " +
                            $"Host: {context.Request.Host} " +
                            $"Path: {context.Request.Path} " +
                            $"QueryString: {context.Request.QueryString} " +
                            $"Response Body: {text}");

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}