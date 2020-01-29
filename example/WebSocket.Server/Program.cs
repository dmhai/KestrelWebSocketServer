using KestrelWebSocketServer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSocket.Server
{
    class Program
    {
        public static IConfigurationRoot configuration { get => SettingTool.AddServerOptionsJsonFile(); }
        public static ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> keyValuePairs = new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

        public static async Task Main(string[] args)
        {
            var ip = configuration.GetValue<string>("ServerOptions:IP");
            var port = configuration.GetValue<int>("ServerOptions:Port");
            var path = configuration.GetValue<string>("ServerOptions:Path");
            var server = new WebSocketServer();

            WebSocketServer.ReceiveBufferSize = 4 * 1024;     //4kb

            await server.BuildAsync(ip, port, path, config =>
            {
                config.OnOpen = (connection, websocket) =>
                {
                    var id = connection.Id;
                    keyValuePairs.TryAdd(id, websocket);
                    Console.WriteLine($"{id} Opened");
                };

                config.OnMessage = async (connection, webSocket, msg) =>
                {
                    var id = connection.Id;
                    Console.WriteLine($"Received {id}: {msg}");

                    if (keyValuePairs.TryGetValue(id, out System.Net.WebSockets.WebSocket value))
                    {
                        await value.SendAsync(msg);
                    }
                };

                config.OnBinary = async (connection, webSocket, file) =>
                {
                    var id = connection.Id;
                    Console.WriteLine($"{id} Binary");

                    using (FileStream fileStream = new FileStream("your file path", FileMode.Create))
                    {
                        await fileStream.WriteAsync(file);
                        fileStream.Flush();
                    }
                };

                config.OnClose = (connection, webSocket) =>
                {
                    var id = connection.Id;
                    keyValuePairs.TryRemove(id, out _);
                    Console.WriteLine($"{id} Closed");
                };

            });

            Console.ReadLine();
            await server.CloseAsync();
        }

    }
}
