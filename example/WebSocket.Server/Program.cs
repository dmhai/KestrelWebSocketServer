using KestrelWebSocketServer;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace WebSocket.Server
{
    class Program
    {
        public static IConfigurationRoot configuration { get => SettingTool.AddServerOptionsJsonFile(); }

        public static async Task Main(string[] args)
        {
            var ip = configuration.GetValue<string>("ServerOptions:IP");
            var port = configuration.GetValue<int>("ServerOptions:Port");
            var path = configuration.GetValue<string>("ServerOptions:Path");
            var server = new WebSocketServer();

            await server.BuildAsync(ip, port, path, config =>
            {
                config.OnOpen = (connection, websocket) =>
                {
                    var id = connection.Id;
                    Console.WriteLine($"{id} Opened");
                };

                config.OnMessage = async(connection, webSocket, msg) =>
                {
                    var id = connection.Id;
                    Console.WriteLine($"Received {id}: {msg}");
                    await webSocket.SendAsync(msg);
                };

                config.OnBinary = (connection, webSocket, file) =>
                {
                    var id = connection.Id;
                    Console.WriteLine($"{id} Binary");
                };

                config.OnClose = (connection, webSocket) =>
                {
                    var id = connection.Id;
                    Console.WriteLine($"{id} Closed");
                };

            });

            Console.ReadLine();
        }

    }
}
