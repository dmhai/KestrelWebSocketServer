using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using System.Text;

namespace KestrelWebSocketServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddLogging(builder =>
            //{
            //    builder.AddConsole()
            //        .AddDebug()
            //        .AddFilter<ConsoleLoggerProvider>(category: null, level: LogLevel.Debug)
            //        .AddFilter<DebugLoggerProvider>(category: null, level: LogLevel.Debug);
            //});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            });

            app.Use(async (context, next) =>
            {
                var path = WebSocketServer.Path;

                if (context.Request.Path == path)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        WebSocketServer.ConfigAction.OnOpen?.Invoke(context.Connection, webSocket);
                        await Process(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private async ValueTask Process(HttpContext context, WebSocket webSocket)
        {
            while (!webSocket.CloseStatus.HasValue)
            {
                await ProcessLine(context, webSocket);
            }

            WebSocketServer.ConfigAction.OnClose?.Invoke(context.Connection, webSocket);
            await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
        }

        private async ValueTask ProcessLine(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var resultMemory = new Memory<byte>(buffer);
            ValueWebSocketReceiveResult result;
            while (true)
            {
                result = await webSocket.ReceiveAsync(resultMemory, CancellationToken.None).ConfigureAwait(false);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            resultMemory = new Memory<byte>(buffer, 0, result.Count);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    {
                        var messageText = Encoding.UTF8.GetString(resultMemory.Span);
                        WebSocketServer.ConfigAction.OnMessage?.Invoke(context.Connection, webSocket, messageText);
                    }
                    break;
                case WebSocketMessageType.Binary:
                    {
                        WebSocketServer.ConfigAction.OnBinary?.Invoke(context.Connection, webSocket, resultMemory);
                    }
                    break;
            }
        }

    }
}