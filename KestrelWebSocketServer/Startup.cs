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

        public MemoryBytePool BytePool = new MemoryBytePool();

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
            var values = await ReceiveFullMsgAsync(webSocket);
            var result = values.Item1;
            var valueMemory = values.Item2;
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    {
                        var messageText = Encoding.UTF8.GetString(valueMemory.Span);
                        WebSocketServer.ConfigAction.OnMessage?.Invoke(context.Connection, webSocket, messageText);
                    }
                    break;
                case WebSocketMessageType.Binary:
                    {
                        WebSocketServer.ConfigAction.OnBinary?.Invoke(context.Connection, webSocket, valueMemory);
                    }
                    break;
            }
        }

        private async ValueTask<ValueTuple<ValueWebSocketReceiveResult, ReadOnlyMemory<byte>>> ReceiveFullMsgAsync(WebSocket webSocket)
        {
            using (var poolMemory = BytePool.GetBuffer(ReceiveBufferSize))
            {
                var buffer = poolMemory.Memory;
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);

                if (result.EndOfMessage)
                {
                    return (result, buffer.Slice(0, result.Count));
                }
                else
                {
                    List<byte> allByte = new List<byte>();
                    allByte.AddRange(buffer.Span.Slice(0, result.Count).ToArray());
                    while (true)
                    {
                        result = await webSocket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                        allByte.AddRange(buffer.Span.Slice(0, result.Count).ToArray());
                        if (result.EndOfMessage)
                        {
                            break;
                        }
                    }

                    return (result, allByte.ToArray());
                }
            }
        }

    }
}