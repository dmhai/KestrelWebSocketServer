using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace KestrelWebSocketServer
{
    public class WebSocketConfigAction
    {
        public Action<ConnectionInfo, WebSocket> OnOpen { get; set; }

        public Action<ConnectionInfo, WebSocket, string> OnMessage { get; set; }

        public Action<ConnectionInfo, WebSocket, Memory<byte>> OnBinary { get; set; }

        public Action<ConnectionInfo, WebSocket> OnClose { get; set; }

        //public Action<> OnPing { get; set; }

        //public Action<> OnPong { get; set; }

        //public Action<> OnError { get; set; }
    }
}
