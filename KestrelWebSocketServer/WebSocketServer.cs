using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KestrelWebSocketServer
{
    public class WebSocketServer
    {
        public static string Path { get; set; } = "/";

        public static WebSocketConfigAction ConfigAction { get; set; }

        public static int ReceiveBufferSize { get; set; } = 4096;

        private IHost WebHost { get; set; }

        public async ValueTask BuildAsync(string ip, int port, string path, Action<WebSocketConfigAction> action)
        {
            if (action == null)
            {
                throw new Exception("action is null");
            }

            Path = path;
            ConfigAction = new WebSocketConfigAction();
            action.Invoke(ConfigAction);
            CreateHost(ip, port, path);
            await WebHost.RunAsync();
        }

        private void CreateHost(string ip, int port, string path)
        {
            WebHost = Host.CreateDefaultBuilder()
                           .ConfigureWebHostDefaults(webBuilder =>
                           {
                               webBuilder.UseKestrel();
                               webBuilder.UseUrls(new string[] { $@"http://{ip}:{port}/" });
                               webBuilder.UseStartup<Startup>();
                           })
                           .Build();
        }

        public async ValueTask CloseAsync()
        {
            if (WebHost != null)
            {
                Startup.BytePool.Dispose();
                await WebHost.StopAsync();
            }
        }

    }
}
