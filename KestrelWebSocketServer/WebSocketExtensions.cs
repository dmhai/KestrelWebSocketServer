﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace KestrelWebSocketServer
{
    public static class WebSocketExtensions
    {
        public static async ValueTask SendAsync(this WebSocket webSocket, string msg)
        {
            var msgByte = new Memory<byte>(Encoding.UTF8.GetBytes(msg));
            await webSocket.SendAsync(msgByte, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async ValueTask CloseAsync(this WebSocket webSocket)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None);
        }

    }
}
