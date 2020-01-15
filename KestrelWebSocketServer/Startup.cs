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
using System.Buffers;
using System.Runtime.InteropServices;

namespace KestrelWebSocketServer
{
    public class Startup
    {
        public int ReceiveBufferSize { get => WebSocketServer.ReceiveBufferSize; } //4*1024 4kb 

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
                ReceiveBufferSize = ReceiveBufferSize
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
            webSocket.Dispose();
        }

        private async ValueTask ProcessLine(HttpContext context, WebSocket webSocket)
        {
            var values = !WebSocketServer.EnabledLargeFileReceive ? await ReceiveFullTextAsync(webSocket) : await ReceiveFullFileAsync(webSocket);
            var result = values.Item1;
            var resultByte = values.Item2;
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    {
                        var messageText = Encoding.UTF8.GetString(resultByte);
                        WebSocketServer.ConfigAction.OnMessage?.Invoke(context.Connection, webSocket, messageText);
                    }
                    break;
                case WebSocketMessageType.Binary:
                    {
                        WebSocketServer.ConfigAction.OnBinary?.Invoke(context.Connection, webSocket, resultByte);
                    }
                    break;
            }
        }

        private async ValueTask<ValueTuple<ValueWebSocketReceiveResult, byte[]>> ReceiveFullTextAsync(WebSocket webSocket)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(ReceiveBufferSize);
            ValueWebSocketReceiveResult result;
            var resulyMemory = new Memory<byte>(buffer);

            while (true)
            {
                result = await webSocket.ReceiveAsync(resulyMemory, CancellationToken.None).ConfigureAwait(false);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            resulyMemory = resulyMemory.Slice(0, result.Count);
            ArrayPool<byte>.Shared.Return(buffer);
            return (result, resulyMemory.ToArray());
        }

        private async ValueTask<ValueTuple<ValueWebSocketReceiveResult, byte[]>> ReceiveFullFileAsync(WebSocket webSocket)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(ReceiveBufferSize);
            ValueWebSocketReceiveResult result;
            List<byte> allByte = new List<byte>();

            while (true)
            {
                result = await webSocket.ReceiveAsync(new Memory<byte>(buffer), CancellationToken.None).ConfigureAwait(false);
                allByte.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                if (result.EndOfMessage)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    break;
                }
            }

            return (result, allByte.ToArray());
        }

    }
}