﻿using Microsoft.AspNetCore.Hosting;
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

        public static int ReceiveBufferSize { get; set; } = 4092;  //4*1024 4kb

        public static bool EnabledLargeFileReceive { get; set; } = false;

        public async Task BuildAsync(string ip, int port, string path, Action<WebSocketConfigAction> action)
        {
            if (action == null)
            {
                throw new Exception("action is null");
            }

            Path = path;
            ConfigAction = new WebSocketConfigAction();
            action.Invoke(ConfigAction);
            await Host.CreateDefaultBuilder()
                      .ConfigureWebHostDefaults(webBuilder =>
                      {
                          webBuilder.UseKestrel();
                          webBuilder.UseLibuv();
                          webBuilder.UseUrls(new string[] { $@"http://{ip}:{port}/" });
                          webBuilder.UseStartup<ProcessHandler>();
                      })
                      .Build()
                      .RunAsync();
        }

    }
}
